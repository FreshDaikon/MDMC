using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daikon.Contracts.Data;
using Daikon.Contracts.Games;
using Daikon.Contracts.Models;
using Daikon.Contracts.Users;
using Daikon.Helpers;
using Godot;
using Newtonsoft.Json;

namespace Daikon.Client.Connect;

public partial class DaikonConnect: Node
{
    public static DaikonConnect Instance;   
    private string _baseLocal = "http://127.0.0.1:5000";
    private string _baseRemote = "http://103.45.246.143:5000";
    private string _baseActual = "";

    private string[] _baseHeader = {"Content-Type: application/json"};
    
    // Various Configs:
    private bool _connectOnline = false;

    //Session Data 
    private Guid _sessionToken;
    private string _joinCode;

    //Signals:
    [Signal]
    public delegate void AuthSuccessEventHandler();
    [Signal]
    public delegate void GameCreatedEventHandler(string host, int port, string joinCode);
    [Signal]
    public delegate void GameFoundEventHandler(string host, int port);


    public override void _Ready()
    {
        if(Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        _baseActual = _baseLocal;
        base._Ready();
    }

    public void SetToLocalHost()
    {
        _baseActual = _baseLocal;
    }

    public void SetToRemote()
    {
        _baseActual = _baseRemote;
    }

    //_____________________________Requests____________________________________________//
    ////////////////////////////////////////////////////////////////////////////////////
    public void DaikonAuthenticate() 
    {
        var authRequest = new HttpRequest();
        AddChild(authRequest);
        authRequest.RequestCompleted += (result, responseCode, headers, body) => {
            if(responseCode != 200)
            {
                GD.Print("Respones was not 200 OK -  [" + responseCode + "]"); 
                CleanUp(authRequest);
                return;
            }
            AuthRequestCompleted(body);
            CleanUp(authRequest);
        }; 
        var message = new AuthUserRequest()
        {
            SteamTicket = "bannana"
        };
        var messageContent = JsonConvert.SerializeObject(message);
        var error = authRequest.Request(_baseActual+"/auth", _baseHeader, HttpClient.Method.Get, messageContent);
    }

    public void DaikonRequestGame(int ArenaId)
    {
        GD.Print("Let's try to get a game! give the ID:" + ArenaId);
        if(_sessionToken == Guid.Empty)
        {
            // We need a valid token to proceed!
            return;
        }
        var gameRequest = new HttpRequest();
        AddChild(gameRequest);
        gameRequest.RequestCompleted += (result, responseCode, headers, body) => {
            if(responseCode != 200)
            {
                GD.Print("Response was not 200 OK -  [" + responseCode + "]"); 
                CleanUp(gameRequest);
                return;
            }
            GameRequestCompleted(body); 
            CleanUp(gameRequest);
        };
        var message = new NewGameRequest()
        {
            Arena = ArenaId.ToString(),
            SessionToken = _sessionToken
        };
        var messageContent = JsonConvert.SerializeObject(message);
        var error = gameRequest.Request(_baseActual+"/games/create", _baseHeader, HttpClient.Method.Get, messageContent);
    }
    
    public void DaikonJoinGame(string joinCode)
    {
        if(_sessionToken == Guid.Empty)
        {
            // We need a valid token to proceed!
            return;
        }
        
        var joinRequest = new HttpRequest();
        AddChild(joinRequest);
        joinRequest.RequestCompleted += (result, responseCode, headers, body) => {
            if(responseCode != 200)
            {
                GD.Print("Respones was not 200 OK -  [" + responseCode + "]"); 
                CleanUp(joinRequest);
                return;
            }
            JoinRequestCompleted(body); 
            CleanUp(joinRequest);
        };
        var message = new JoinGameRequest()
        {
            SessionToken = _sessionToken,
            JoinCode = joinCode
        };
        var messageContent = JsonConvert.SerializeObject(message);
        var error = joinRequest.Request(_baseActual+"/games/join", _baseHeader, HttpClient.Method.Get, messageContent);
    }

    public void GetArenaRecords(int arenaId)
    {
        var arenaRecordRequest = new HttpRequest();
        AddChild(arenaRecordRequest);
        
        arenaRecordRequest.RequestCompleted += (result, responseCode, headers, body) => 
        {
            if(responseCode != 200)
            {
                GD.Print("Responses was not 200 OK -  [" + responseCode + "]"); 
                CleanUp(arenaRecordRequest);
                return;
            }
            GetArenaRecordResponse(body); 
            CleanUp(arenaRecordRequest);
        };
        var message = new GetArenaRecordsRequest()
        {
            SessionToken = _sessionToken,
            ArenaId = arenaId
        };
        var messageContent = JsonConvert.SerializeObject(message);
        var error = arenaRecordRequest.Request(_baseActual+"/data/GetArenaRecords", _baseHeader, HttpClient.Method.Get, messageContent);
    }

    //___________________________ Responses ____________________________________________//
    ////////////////////////////////////////////////////////////////////////////////////

    private void AuthRequestCompleted(byte[] body)
    {
        var json = Encoding.UTF8.GetString(body);
        var messageData = JsonConvert.DeserializeObject<AuthUserResponse>(json); 
        GD.Print(messageData);        
        GD.Print("We good to go!");
        _sessionToken = messageData.SessionToken;
        EmitSignal(SignalName.AuthSuccess);
    }

    private void GameRequestCompleted(byte[] body)
    {
        var json = Encoding.UTF8.GetString(body);
        var messageData = JsonConvert.DeserializeObject<GameCreatedResponse>(json); 
        GD.Print(messageData);  
        // Setup Multiplayer interface:
        GD.Print(messageData.JoinCode);
        EmitSignal(SignalName.GameCreated, new Variant[]{ messageData.ServerHost, messageData.ServerPort, messageData.JoinCode });
    }

    private void JoinRequestCompleted(byte[] body)
    {
        var json = Encoding.UTF8.GetString(body);
        var messageData = JsonConvert.DeserializeObject<GameFoundReponse>(json); 
        GD.Print(messageData);  
        EmitSignal(nameof(GameFound), new Variant[]{messageData.ServerHost, messageData.ServerPort});
    }

    private void GetArenaRecordResponse(byte[] body)
    {
        var json = Encoding.UTF8.GetString(body);
        var messageData = JsonConvert.DeserializeObject<GetArenaRecordsResponse>(json);
        GD.Print("Arena Record Data  :" + messageData);
        foreach (var arenaRecord in messageData.Records.ToList())
        {
            GD.Print(arenaRecord.ToString());
        }
    }

    //___________________________ Utility ____________________________________________//
    ////////////////////////////////////////////////////////////////////////////////////
    private void CleanUp(Node request)
    {
        RemoveChild(request);
        request.QueueFree();
    }

}