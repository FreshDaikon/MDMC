using System;
using Godot;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.Combat.ArsenalSystem;
using Mdmc.Code.Game.Entity.Player.Components;
using static System.Int32;

namespace Mdmc.Code.Game.Entity.Player;

public partial class PlayerEntity : Entity
{
    public bool IsLocalPlayer;
    //Private Player  members:
    [Export] public PlayerCamera Camera { get; private set; }
    [Export] public PlayerInput Input { get; private set; }
    [Export] public PlayerArsenal Arsenal { get; private set; } 
    [Export] public PlayerMover Mover { get; private set; }

    public int Id { get; set; }

    public Entity CurrentFriendlyTarget { get; private set; }

    public void ChangeFriendlyTarget(Entity target)
    {
        CurrentFriendlyTarget = target;
        GD.Print("Friend ID:" + target.Name);
        Rpc(nameof(SyncFriendlyTarget), Parse(target.Name));
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncFriendlyTarget(int id)
    {
        var entities = ArenaManager.Instance.GetCurrentArena().GetEntities();
        if(entities.Count > 0)
        {
            var entity = entities.Find(e => Parse(e.Name) == id );
            if(entity != null) CurrentFriendlyTarget = entity;
            return;
        }
        CurrentFriendlyTarget = null;
    }

    public override void _Ready()
    {
        //Make Sure the Input Component is owned by the client peer.
        Input.SetMultiplayerAuthority(Parse(Name));
        
        if(Multiplayer.GetUniqueId() == Parse(Name))
		{
			IsLocalPlayer = true;
            Camera.GetCamera().Current = true;
            
            GameManager.Instance.ConnectionStarted += () => {
                RpcId(1, nameof(Reset));
            };

		}
        else if(Multiplayer.GetUniqueId() == 1)
        {
            Camera.QueueFree();
        }
        else
        {
            Camera.QueueFree();
            Input.QueueFree();
            Mover.QueueFree();
        }
             
        base._Ready();
    }
    public override void _Process(double delta)
    {
        if(!Multiplayer.IsServer())
            return;
        
        base._Process(delta);
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void Reset()
    {
        //Just reset arsenal for now:
        Arsenal.ResetArsenal();
    }
}