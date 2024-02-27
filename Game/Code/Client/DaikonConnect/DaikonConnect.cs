using System;
using System.Text;
using Daikon.Contracts.Games;
using Daikon.Contracts.Users;
using Daikon.Helpers;
using Godot;
using Newtonsoft.Json;

namespace Daikon.Client.Connect;

public partial class DaikonConnect: Node
{
    public static DaikonConnect Instance;   
    private string baseLocal = "http://127.0.0.1:5000";
    private string baseRemote = "http://194.163.183.217:5000";
    private string baseActual = "";

    private string[] baseHeader = {"Content-Type: application/json"};
    
    // Various Configs:
    public bool ConnectOnline = false;

    //Session Data 
    private Guid SessionToken;
    private string JoinCode;

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
        baseActual = baseLocal;
        base._Ready();
    }

    public void SetToLocalHost()
    {
        baseActual = baseLocal;
    }

    public void SetToRemote()
    {
        baseActual = baseRemote;
    }

    //_____________________________Requests____________________________________________//
    ////////////////////////////////////////////////////////////////////////////////////
        
    
    // Authorize and recieve a token:
    public void DaikonAuth() 
    {
        HttpRequest authRequest = new HttpRequest();
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
        AuthUserRequest message = new AuthUserRequest()
        {
            // TODO : Implement the proper steam x-ticket.
            SteamTicket = "bannana"
        };
        var messageContent = JsonConvert.SerializeObject(message);
        Error error = authRequest.Request(baseActual+"/auth", baseHeader, HttpClient.Method.Get, messageContent);
    }

    public void DaikonRequestGame(int ArenaId)
    {
        GD.Print("Let's try to get a game! give the ID:" + ArenaId);
        if(SessionToken == Guid.Empty)
        {
            // We need a valid token to proceed!
            return;
        }
        HttpRequest gameRequest = new HttpRequest();
        AddChild(gameRequest);
        gameRequest.RequestCompleted += (result, responseCode, headers, body) => {
            if(responseCode != 200)
            {
                GD.Print("Respones was not 200 OK -  [" + responseCode + "]"); 
                CleanUp(gameRequest);
                return;
            }
            GameRequestCompleted(body); 
            CleanUp(gameRequest);
        };
        NewGameRequest message = new NewGameRequest()
        {
            Arena = ArenaId.ToString(),
            SessionToken = SessionToken
        };
        var messageContent = JsonConvert.SerializeObject(message);
        Error error = gameRequest.Request(baseActual+"/games/create", baseHeader, HttpClient.Method.Get, messageContent);
    }

    public void DaikonJoinGame(string joinCode)
    {
        if(SessionToken == Guid.Empty)
        {
            // We need a valid token to proceed!
            return;
        }
        
        HttpRequest joinRequest = new HttpRequest();
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
        JoinGameRequest message = new JoinGameRequest()
        {
            SessionToken = SessionToken,
            JoinCode = joinCode
        };
        var messageContent = JsonConvert.SerializeObject(message);
        Error error = joinRequest.Request(baseActual+"/games/join", baseHeader, HttpClient.Method.Get, messageContent);
    }

    //___________________________ Responses ____________________________________________//
    ////////////////////////////////////////////////////////////////////////////////////

    private void AuthRequestCompleted(byte[] body)
    {
        var json = UTF8Encoding.UTF8.GetString(body);
        var messageData = JsonConvert.DeserializeObject<AuthUserResponse>(json); 
        GD.Print(messageData);        
        GD.Print("We good to go!");
        SessionToken = messageData.SessionToken;
        EmitSignal(SignalName.AuthSuccess);
    }

    private void GameRequestCompleted(byte[] body)
    {
        var json = UTF8Encoding.UTF8.GetString(body);
        var messageData = JsonConvert.DeserializeObject<GameCreatedResponse>(json); 
        GD.Print(messageData);  
        // Setup Multiplayer interface:
        GD.Print(messageData.JoinCode);
        EmitSignal(SignalName.GameCreated, new Variant[]{ messageData.ServerHost, messageData.ServerPort, messageData.JoinCode });
    }

    private void JoinRequestCompleted(byte[] body)
    {
        var json = UTF8Encoding.UTF8.GetString(body);
        var messageData = JsonConvert.DeserializeObject<GameFoundReponse>(json); 
        GD.Print(messageData);  
        EmitSignal(nameof(GameFound), new Variant[]{messageData.ServerHost, messageData.ServerPort});
    }

    //___________________________ Utility ____________________________________________//
    ////////////////////////////////////////////////////////////////////////////////////
    
    private void CleanUp(HttpRequest request)
    {
        RemoveChild(request);
        request.QueueFree();
    }

}