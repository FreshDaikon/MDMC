using Godot;
using Daikon.Game;
using Daikon.Helpers;

namespace Daikon.Client;

public partial class ClientMultiplayerManager: Node
{

    public static ClientMultiplayerManager Instance;
    private ENetMultiplayerPeer _clientPeer;

    private string _hostUrl;
    private int _hostPort;
    private string _gameId;
    private bool _hasData = false;
    private bool _hasId = false;

    public int LocalPid = -1;
    public bool HasLocalServer = false;

    [Signal]
    public delegate void ConnectedToServerEventHandler();
    [Signal]
    public delegate void DisconnectedFromServerEventHandler();

    public override void _Ready()
    {
        if(Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        _clientPeer = new ENetMultiplayerPeer();
       
    }

    public void SetData(string url, int port)
    {   
        _hostPort = port;
        _hostUrl = url;
        _hasData = true;
    }

    public void SetId(string id)
    {
        _gameId = id;
        _hasId = true;
    }

    public string GetId()
    {
        if(_hasId)
        {
            return _gameId;
        }
        else
        {
            return "";
        }
    }

    public bool StartPeer()
    {
        if(!_hasData)  
            return false;
        // Continue:
        Engine.MaxFps = 120;
        GD.Print("Prepare MP Client - connect to server...");
        var error = _clientPeer.CreateClient(_hostUrl, _hostPort);
        if(error != Error.Ok)
        {
            OS.Alert("Failed to start Client [" + error.ToString() +"]");
            return false; 
        }
        else 
        {
            Multiplayer.MultiplayerPeer = _clientPeer;
            Multiplayer.ServerDisconnected += () => StopPeer(); 
            GD.Print("Client Connected to Server! As : " + Multiplayer.MultiplayerPeer ); 
            return true;
        }
    }

    public void StopPeer()
    {
        ClientManager.Instance.ResetClient();
        MD.Log("Server was disconnected clear info..");
        _clientPeer.Close();
        GetTree().SetMultiplayer(MultiplayerApi.CreateDefaultInterface());
        _hasData = false;
        if(LocalPid != -1)
        {
            MD.Log("Server Connection was local - clear LocalData.");
            LocalPid = -1;
            HasLocalServer = false;
        }
    }

    public void LeaveServer()
    {
        if(_clientPeer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected)
        {
            MD.Log("Time to leave the server!");
            _clientPeer.Close();
            _hasData = false;
            ClientManager.Instance.ResetClient();
            GetTree().SetMultiplayer(MultiplayerApi.CreateDefaultInterface());
            if(LocalPid != -1)
            {
                MD.Log("Server Connection was local - clear LocalData.");
                LocalPid = -1;
                HasLocalServer = false;
            }
        }
        else
        {
            MD.Log("Can't Leave Server becaues ther is no connection.");
            return;
        }
    }

    public MultiplayerPeer.ConnectionStatus GetStatus()
    {
        if(_clientPeer != null)
        {
            return _clientPeer.GetConnectionStatus();
        }
        else 
            return MultiplayerPeer.ConnectionStatus.Disconnected;
    }

    // Only works with a local copy of the x86 - dont try this on retail distros.
    public void StartLocalServer(Arena arena)
    {
        var args = MD.GetArgs();
        var path = OS.GetExecutablePath();
        GD.Print("Trying to start local Server..");
        if(args.ContainsKey("vscode"))
        {
            var projPath = path.Replace(".Engine/Godot.exe", "Game"); 
            GD.Print("Project path :" + projPath);
            path = path.Replace("Godot.exe", "Godot_console.exe");
            GD.Print("Exe Path:" + path);
            var pid = OS.CreateProcess(path, new string[]{"--path", projPath,"--headless", "--gameserver", "--arena "+ arena.Id.ToString()}, true);
            if(pid != -1)
            {
                LocalPid = pid;
                HasLocalServer = true;
            }
        }
        else
        {
            path = path.Replace("MDMC.exe", "MDMC_Server.console.exe");
            OS.CreateProcess(path + "/Server/MDMC_Server.console.exe", 
                new string[]{" --headless", "--arena "+ arena.Id.ToString()},
                true);
        }
    }
}