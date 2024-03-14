using System;
using Godot;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.Combat.ArsenalSystem;
using Mdmc.Code.Game.Entity.Player.Components;
using static System.Int32;

namespace Mdmc.Code.Game.Entity.Player;

public partial class PlayerEntity : Entity
{
    public bool IsLocalPlayer = false;
    //Private Player  members:
    private PlayerInput input;
    private PlayerArsenal arsenal;
    private PlayerCamera camera;
    private PlayerMover mover;

    public PlayerArsenal Arsenal { get {return arsenal;}}
    public PlayerMover Mover { get {return mover; }}

    [Export]
    public int FriendlyTargetId = -1;
    public Entity CurrentFriendlyTarget { get; private set; }

    public void ChangeFriedlyTarget(Entity target)
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

    public PlayerInput playerInput
    {
        get { return input; }
    }

    public override void _Ready()
    {
        input = GetNode<PlayerInput>("%Input");
        arsenal = GetNode<PlayerArsenal>("%Arsenal");
        camera = GetNode<PlayerCamera>("%Rig");
        mover = GetNode<PlayerMover>("%Mover");
        //Make Sure the Input Component is owned by the client peer.
        input.SetMultiplayerAuthority(Parse(Name));
        
        if(Multiplayer.GetUniqueId() == Parse(Name))
		{
			IsLocalPlayer = true;
            camera.GetCamera().Current = true;
            
            GameManager.Instance.ConnectionStarted += () => {
                RpcId(1, nameof(Reset));
            };
            //mover.QueueFree();

		}
        else if(Multiplayer.GetUniqueId() == 1)
        {
            camera.QueueFree();
        }
        else
        {
            camera.QueueFree();
            input.QueueFree();
            mover.QueueFree();
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
        arsenal.ResetArsenal();
    }
}