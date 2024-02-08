using Godot;

public partial class ClientMultiplayerManager: Node
{

    public static ClientMultiplayerManager Instance;
    private ENetMultiplayerPeer _clientPeer;

    private string _hostUrl;
    private int _hostPort;
    private bool _hasData;

    public override void _Ready()
    {
        if(Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
    }

    public void SetData(string url, int port)
    {   
        _hostPort = port;
        _hostUrl = url;
        _hasData = true;
    }

    public bool StartPeer()
    {
        if(!_hasData)  
            return false;
        // Continue:
        Engine.MaxFps = 120;
        GD.Print("Prepare MP Client - connect to server...");
        _clientPeer = new ENetMultiplayerPeer();
        var error = _clientPeer.CreateClient(_hostUrl, _hostPort);
        if(error != Error.Ok)
        {
            OS.Alert("Failed to start Client [" + error.ToString() +"]");
            return false; 
        }
        else 
        {
            Multiplayer.MultiplayerPeer = _clientPeer;
            GD.Print("Client Connected to Server! As : " + Multiplayer.MultiplayerPeer ); 
            return true;
        }
    }

    public bool StopPeer()
    {
        if(_clientPeer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected)
        {
            _clientPeer.Close();
            _hasData = false;
            return true;
        }
        return false;
    }

    // Only works with a local copy of the x86 - dont try this on retail distros.
    public void StartLocalServer(Arena arena)
    {
        var path = OS.GetExecutablePath();
        var pid = OS.Execute(path + "/Server/Server.exe", new string[]{" --headless", "--arena "+ arena.Id.ToString()});
    }
}