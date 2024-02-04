using Godot;
using PlayFab.MultiplayerModels;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;


public partial class WSClient : Node
{
    public static WSClient Instance;

    private WebSocketPeer _wsClient;
    private const string LocalTestHost = "ws://localhost:5000/Orch";
    private const string WWHost = "wss://mdmc-orchestrator.azurewebsites.net/Orch";
    private const string VPShost = "ws://194.163.183.217:5001/Orch";
    private bool State = false;

    public override void _Ready()
    {
        base._Ready();
        if(Instance != null)
        {
            Free();
            return;
        }                
        Instance = this;
        ConnectToOrchestrator();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(State)
        {
            _wsClient?.Poll();
            var state = _wsClient.GetReadyState();
            switch(state)
            {
                case WebSocketPeer.State.Open:
                    if(_wsClient.GetAvailablePacketCount() > 0)
                    {
                        MD.Log("WS Polling..");
                        HandleDataReceived(); 
                    }
                    break;
                case WebSocketPeer.State.Closing:
                    MD.Log("WS Closing..");
                    break;
                case WebSocketPeer.State.Connecting:
                    MD.Log("WS Connecting.."); 
                    break;
                case WebSocketPeer.State.Closed:
                    MD.Log("WS Closed..");
                    State = false;
                    break;
            }
        }
    }

    public override void _ExitTree()
    {
        _wsClient.Close();
        base._ExitTree();
    }

    public void Connect()
    {
        if(State == false)
        {
            ConnectToOrchestrator();
        }
        else
        { 
            MD.Log("Connection already going.");
        }
    }

    private void ConnectToOrchestrator()
    {
        _wsClient = new WebSocketPeer();
        string key = "testclientkey123";
        var _connHeader = new string[]{$"keyToken: {key}"};
        _wsClient.HandshakeHeaders = _connHeader;
        //Connect:
        _wsClient.ConnectToUrl(VPShost);
        State = true;
    }

    private void HandleDataReceived() 
    {
        var packet = _wsClient.GetPacket();
        bool isString = _wsClient.WasStringPacket();
        if (isString) 
        {
            var msg = UTF8Encoding.UTF8.GetString(packet);
            HandleMessageReceived(msg);
        }
    }

    private Error SendData(dynamic message) 
    {
        var messageContent = JsonSerializer.Serialize(message);
        return _wsClient.PutPacket(Encoding.UTF8.GetBytes(messageContent));
    }

    public void RequestServer()
    {        
        SendData(new {
            MessageType = ClientMessageType.RequestGame,
            MessageContent = new RequestGameMessage()
            {
                Arena = "Test_Arena"
            }
        });
    }

    public void JoinServer(string JoinCode)
    {
        SendData(new {
            MessageType = ClientMessageType.JoinGame,
            MessageContent = new JoinGameMessage(){
                ShareableCode = JoinCode
            }
        });
    }

    private void HandleMessageReceived(string message) {
        var messageData = JsonSerializer.Deserialize<JsonObject>(message);
        switch ((OrchMessageType) (int) messageData["MessageType"]) {
            case OrchMessageType.ConnectionEstablished:
                MD.Log("Connection Established!");
                var establishedMessage = messageData["MessageContent"].Deserialize<ConnectionEstablishedMessage>();
                MD.Log("Message was: " + establishedMessage.WelcomeMessage);
                break;
            case OrchMessageType.GameRequestFailed:
                MD.Log("Server Request Failed");
                var requestFailed = messageData["MessageContent"].Deserialize<GameRequstFailedMessage>();
                MD.Log("Message was: " + requestFailed.Reason);
                break;
            case OrchMessageType.GameCreated:
                MD.Log("Server Created!");
                var createdMessage = messageData["MessageContent"].Deserialize<GameCreatedMessage>();
                MD.Log("Host is: " + createdMessage.ServerUrl );
                MD.Log("Port is: " + createdMessage.ServerPort );
                MD.Log("Code is: " + createdMessage.ShareableCode );
                ClientManager.Instance.StartAsClient(createdMessage.ServerUrl, createdMessage.ServerPort);
                FrontendMain.Instance.SetState(false);
                break;
            case OrchMessageType.GameFound:
                MD.Log("Server Found!");
                var foundMessage = messageData["MessageContent"].Deserialize<GameFoundMessage>(); 
                MD.Log("Host is: " + foundMessage.ServerUrl );
                MD.Log("Port is: " + foundMessage.ServerPort );
                ClientManager.Instance.StartAsClient(foundMessage.ServerUrl, foundMessage.ServerPort);
                FrontendMain.Instance.SetState(false);
                break;
        }
    }   
}