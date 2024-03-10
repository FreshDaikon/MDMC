using System;
using Godot;
using Mdmc.Code.Game;
using Mdmc.Code.Game.Entity.Player;
using ArenaManager = Mdmc.Code.Game.Arena.ArenaManager;

namespace Mdmc.Code.Server;

public partial class ServerManager : Node3D
{	
    public static ServerManager Instance;
    public string ConnectionType { get; private set;}
    private ENetMultiplayerPeer peer;
   
    private Node3D EntityContainer;
    [Export]
    private string playerEntityPath;

    public override void _Ready()
    {  
        if(Instance != null)
        {
            Free();
            return;
        }
        Instance = this;        
    }

    public override void _Notification(int what)
    {
        if(what == NotificationWMCloseRequest)
        {
            foreach(var player in Multiplayer.GetPeers())
            {
                if(player != Multiplayer.GetUniqueId())
                {
                    peer.DisconnectPeer(player);
                }
            }
            GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
            GetTree().Quit();
        }
    }

    public override void _Process(double delta)
    {
        var currentArena = ArenaManager.Instance.GetCurrentArena();
        if(currentArena != null)
        {
            if(currentArena.GetTimeLeft() <= 0)
            {
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

    public void StartServer(int port, int maxPlayers, string arena)
    {
        Engine.MaxFps = 30;
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
                GameManager.Instance.StartGame(true);
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

    // SIGNALS:
    private void PeerDisconnected(long id)
    {
        var arena = ArenaManager.Instance.GetCurrentArena();
        if(arena != null)
        {
            arena.RemovePlayerEntity ((int)id);
        }
        if(Multiplayer.GetPeers().Length <= 0)
        {
            GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
            GetTree().Quit();
        }

    }
    private void PeerConnected(long id)
    {
        if(ArenaManager.Instance.HasArena())
        {   
            var arena = ArenaManager.Instance.GetCurrentArena();
            var newPlayer = CreateNewPlayer(id);
            arena.AddPlayerEntity(newPlayer);
        }
        else 
        {
            peer.DisconnectPeer((int)id);
        }
    }

    private PlayerEntity CreateNewPlayer(long id)
    {
        var prefab = (PackedScene)ResourceLoader.Load(playerEntityPath);
        PlayerEntity player = prefab.Instantiate<PlayerEntity>();
        player.Name =  id.ToString();
        player.EntityName = string.Concat("Player", id.ToString().AsSpan(0, 4));
        return player;        
    }
}
