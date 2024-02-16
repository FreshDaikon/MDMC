using Godot;
using Daikon.Game;
using Daikon.System;

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
        var args = MD.GetArgs();
        var path = OS.GetExecutablePath();
        GD.Print("Trying to start local Server..");
        if(args.ContainsKey("vscode"))
        {
            var projPath = path.Replace(".Engine/Godot.exe", "Game"); 
            GD.Print("Project path :" + projPath);
            path = path.Replace("Godot.exe", "Godot_console.exe");
            GD.Print("Exe Path:" + path);
            OS.CreateProcess(path, new string[]{"--path", projPath,"--headless", "--gameserver", "--arena "+ arena.Id.ToString()}, true);
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