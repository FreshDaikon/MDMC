using Godot;
using Microsoft.Playfab.Gaming.GSDK.CSharp;
using System;
using System.Linq;
using System.Net;
using System.Threading;

public partial class ServerManager : Node3D
{	
    public static ServerManager Instance;
    public string ConnectionType { get; private set;}
    private ENetMultiplayerPeer peer;
   
    private Node3D EntityContainer;
    [Export]
    private string playerEntityPath;
    [Export]
    private string sackManEntityPath;

    public override void _Ready()
    {  
        if(Instance != null)
        {
            Free();
            return;
        }
        Instance = this;        
        GD.Print("Instance Cleared... ");        
    }

    public override void _Process(double delta)
    {
        var currentArena = ArenaManager.Instance.GetCurrentArena();

        if(currentArena != null)
        {
            if(currentArena.GetTimeLeft() <= 0)
            {
                MD.Log(MD.Runtime.Server, "ServerManager", "Arena Time is up! - Closing Down..");
                foreach(var player in Multiplayer.GetPeers())
                {
                    if(player != Multiplayer.GetUniqueId())
                    {
                        peer.DisconnectPeer(player);
                    }
                }
            }
        }
        base._Process(delta);
    }

    public void StartAsStandaloneServer(int port, int maxPlayers)
    {
        Engine.MaxFps = 12;
        GD.Print("Starting Server...");
        //Connect Signals:
        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected; 
        peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(port, maxPlayers);   
        if(error != Error.Ok)
        {
            GD.Print("Can't Host : " + error.ToString());
            return;
        }
        Multiplayer.MultiplayerPeer = peer;   
        GD.Print("Server Started! Awaiting Clients...");        
    }

    public void StartAsStandaloneServer(int port, int maxPlayers, string arena)
    {
        Engine.MaxFps = 60;
        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(port, maxPlayers);   
        if(error != Error.Ok)
        {
            GD.Print("Can't Host : " + error.ToString());
            return;
        }
        Multiplayer.MultiplayerPeer = peer;   
        if(arena != "-1")
        {
            //Arena Setup:
            int id = int.Parse(arena);
            if(ArenaManager.Instance.LoadArena(id))
            {
                //When done:
                GD.Print("Server Started with Arena! Awaiting Clients..."); 
                return;
            }
            else 
            {
                GD.Print("Could Not Load Arena - abort!"); 
                return;
            }
        }
        else
        {
            GD.Print("Server Started WITHOUT Arena!! Awaiting Clients..."); 
            return;
        }
    }

    public bool StartAsPlayfabServer(string cookie, int port, int maxPlayers)
    {
        Engine.MaxFps = 60;
        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        MD.Log(MD.Runtime.Server, "", cookie);
        peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(port, maxPlayers);   
        if(error != Error.Ok)
        {
            GD.Print("Can't Host : " + error.ToString());
            return false;
        }
        Multiplayer.MultiplayerPeer = peer;   
        if(cookie != "-1")
        {
            //Arena Setup:
            int id = int.Parse(cookie);
            if(ArenaManager.Instance.LoadArena(id))
            {
                //When done:
                GD.Print("Server Started! Awaiting Clients..."); 
                return true;
            }
            else 
            {
                GD.Print("Could Not Load Arena - abort!"); 
                return false;
            }
        }
        else
        {
            GD.Print("Server Started! Awaiting Clients..."); 
            return true;
        }
    }

    // SIGNALS:
    private void PeerDisconnected(long id)
    {
        MD.Log(MD.Runtime.Server, "","Player Disconnected...");
        var arena = ArenaManager.Instance.GetCurrentArena();
        if(arena != null)
        {
            arena.RemovePlayerEntity ((int)id);
        }
        MD.Log(MD.Runtime.Server, "Remaining Peers: ", Multiplayer.GetPeers().Length.ToString());
        if(Multiplayer.GetPeers().Length <= 0)
        {
            MD.Log(MD.Runtime.Server, "*", "No Peers remaining after last disconnect - shutting down.");
            GetTree().Quit();
        }

    }
    private void PeerConnected(long id)
    {
        MD.Log(MD.Runtime.Server, "","Player Connnected...");
        if(ArenaManager.Instance.HasArena())
        {   
            var arena = ArenaManager.Instance.GetCurrentArena();
            var newPlayer = CreateNewPlayer(id);
            arena.AddPlayerEntity(newPlayer);
        }
        else 
        {
            MD.Log(MD.Runtime.Server, "","Could not add player - arena is null.. Disconnect Player:");
            peer.DisconnectPeer((int)id);
        }
    }

    private PlayerEntity CreateNewPlayer(long id)
    {
        var prefab = (PackedScene)ResourceLoader.Load(playerEntityPath);
        PlayerEntity player = prefab.Instantiate<PlayerEntity>();
        //Sets the node name . TODO : add more identifying information (steam name fx.)
        player.Name =  id.ToString();
        player.EntityName = "Unknown Player" + id.ToString().Substring(0, 4);
        return player;        
    }
}
