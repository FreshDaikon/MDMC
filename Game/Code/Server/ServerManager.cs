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
        EntityContainer = GetNode<Node3D>("%Entities");
        GD.Print("Instance Cleared... ");
        
    }

    public override void _Process(double delta)
    {
        var currentArena = ArenaManager.Instance.GetCurrentArena();
        if(!currentArena.GetArenaFailedState())
        {
            foreach(var player in Multiplayer.GetPeers())
            {
                peer.DisconnectPeer(player);
            }
        }
        base._Process(delta);
    }

    public void StartAsStandaloneServer(int port, int maxPlayers)
    {
        Engine.MaxFps = 60;
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

    public bool StartAsPlayfabServer(string cookie, int port, int maxPlayers)
    {
        Engine.MaxFps = 60;
        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        MD.Log(cookie);
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
        GD.Print("Peer Disconnected : " + id);
        RemovePlayer(id);
        MD.Log(Multiplayer.GetPeers().Length.ToString());
        if(Multiplayer.GetPeers().Length <= 0)
        {
            GetTree().Quit();
        }

    }
    private void PeerConnected(long id)
    {
        GD.Print("Peer Connected : " + id);
        AddPlayer(id);
    }
    private void AddPlayer(long id)
    {
        var prefab = (PackedScene)ResourceLoader.Load(playerEntityPath);
        PlayerEntity player = prefab.Instantiate<PlayerEntity>();
        int index = 0;
        if(EntityContainer.GetChildCount() > 0)
        {
            index = EntityContainer.GetChildren().Where(p => p is PlayerEntity).Cast<PlayerEntity>().ToList().Count;
        }       
        if(index == 0)
            AddSackMan();
        player.Name = id.ToString();
        player.EntityName = "Cool Player" + id.ToString().Substring(0, 4);
        EntityContainer.AddChild(player);
    }

    private void RemovePlayer(long id)
    {
       var player = EntityContainer.GetNode(id.ToString());
       if(player != null)
       {
            player.QueueFree();
       }
    }

    private void AddSackMan()
    {   
        var prefab = (PackedScene)ResourceLoader.Load(sackManEntityPath);
        Entity sack = prefab.Instantiate<Entity>();
        sack.Name = ((int)ResourceUid.CreateId()).ToString();
        EntityContainer.AddChild(sack);
    }
}
