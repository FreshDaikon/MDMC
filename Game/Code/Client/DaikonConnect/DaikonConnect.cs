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
    private string baseLocal = "http://localhost:5000";
    private string baseRemote = "http://194.163.183.217:5000";
    private string baseActual = "";

    private string[] baseHeader = {"Content-Type: application/json"};
    
    // Various Configs:
    public bool ConnectOnline = false;


    //Session Data 
    private Guid SessionToken;
    private string JoinCode;

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

    public void DaikonAuth() 
    {
        MD.Log("Let's auth this motha..");
        HttpRequest authRequest = new HttpRequest();
        AddChild(authRequest);
        authRequest.RequestCompleted += AuthRequestCompleted;
        AuthUserRequest message = new AuthUserRequest()
        {
            SteamTicket = "bannana"
        };
        var messageContent = JsonConvert.SerializeObject(message);
        Error error = authRequest.Request(baseActual+"/auth", baseHeader, HttpClient.Method.Get, messageContent);
    }

    public void DaikonRequestGame(int ArenaId)
    {
        if(SessionToken == Guid.Empty)
        {
            return;
        }
        HttpRequest gameRequest = new HttpRequest();
        AddChild(gameRequest);
        gameRequest.RequestCompleted += AuthRequestCompleted;
        NewGameRequest message = new NewGameRequest()
        {
            Arena = ArenaId.ToString(),
            SessionToken = SessionToken
        };
        var messageContent = JsonConvert.SerializeObject(message);
        Error error = gameRequest.Request(baseActual+"/games/create", baseHeader, HttpClient.Method.Get, messageContent);
    }

    private void AuthRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
    {
        if(responseCode != 200)
        {
            MD.Log("daymn..we got : " + responseCode); 
        }
        var json = UTF8Encoding.UTF8.GetString(body);
        var messageData = JsonConvert.DeserializeObject<AuthUserResponse>(json); 
        GD.Print(messageData);        
        SessionToken = messageData.SessionToken;
    }

    private void GameRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
    {
        if(responseCode != 200)
        {
            MD.Log("daymn..we got : " + responseCode); 
        }
        var json = UTF8Encoding.UTF8.GetString(body);
        var messageData = JsonConvert.DeserializeObject<AuthUserResponse>(json); 
        GD.Print(messageData);  
    }
}