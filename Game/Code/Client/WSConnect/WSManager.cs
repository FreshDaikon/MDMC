using Godot;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Daikon.Game;
using Daikon.System;

namespace Daikon.Client;

public partial class WSManager : Node
{
    public static WSManager Instance;

    private WebSocketPeer _wsClient;
    private const string LocalTestHost = "ws://localhost:5000/Orch";
    private const string WWHost = "wss://mdmc-orchestrator.azurewebsites.net/Orch";
    private const string VPShost = "ws://194.163.183.217:5001/Orch";
    private bool _running = false;

    public MD.WSConnectionState State = MD.WSConnectionState.Closed;



    public override void _Ready()
    {
        if(Instance != null)
        {
            Free();
            return;
        }                
        Instance = this;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(_running)
        {
            _wsClient?.Poll();
            var state = _wsClient.GetReadyState();
            switch(state)
            {
                case WebSocketPeer.State.Open:
                    State = MD.WSConnectionState.Open;
                    if(_wsClient.GetAvailablePacketCount() > 0)
                    {
                        MD.Log("WS Polling..");
                        HandleDataReceived(); 
                    }
                    break;
                case WebSocketPeer.State.Closing:
                    State = MD.WSConnectionState.Closing;
                    MD.Log("WS Closing..");
                    break;
                case WebSocketPeer.State.Connecting:
                    State = MD.WSConnectionState.Connecting;
                    MD.Log("WS Connecting.."); 
                    break;
                case WebSocketPeer.State.Closed:
                    State = MD.WSConnectionState.Closed;
                    MD.Log("WS Closed..");
                    _running = false;
                    break;
            }
        }
    }

    public override void _ExitTree()
    {
        if(_running)
        {
            _wsClient.Close();
        }
        base._ExitTree();
    }

    public bool ConnectToOrchestrator()
    {
        if(_running)
        {
            MD.Log("WS Already Going..");
            return false;
        }
        _wsClient = new WebSocketPeer();        
        string key = "testclientkey123";
        var _connHeader = new string[]{$"keyToken: {key}"};
        _wsClient.HandshakeHeaders = _connHeader;
        //Connect:
        _wsClient.ConnectToUrl(VPShost);
        _running = true;
        return true;
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

    public void RequestGame(Arena arena)
    {        
        var id = arena.Id.ToString();
        SendData(new {
            MessageType = ClientMessageType.RequestGame,
            MessageContent = new RequestGameMessage()
            {
                Arena = id
            }
        });
    }

    public void JoinGame(string JoinCode)
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
                ClientMultiplayerManager.Instance.SetId(createdMessage.ShareableCode);
                ClientMultiplayerManager.Instance.SetData(createdMessage.ServerUrl, createdMessage.ServerPort);
                break;
            case OrchMessageType.GameFound:
                MD.Log("Server Found!");
                var foundMessage = messageData["MessageContent"].Deserialize<GameFoundMessage>(); 
                MD.Log("Host is: " + foundMessage.ServerUrl );
                MD.Log("Port is: " + foundMessage.ServerPort );
                ClientMultiplayerManager.Instance.SetData(foundMessage.ServerUrl, foundMessage.ServerPort);
                break;
        }
    }   
}