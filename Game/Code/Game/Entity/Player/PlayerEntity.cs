using System;
using Godot;

/// <summary>
/// Extends the basic Entity System + EntityData. <br>
/// Should be handled by a MultiplayerSpawner (GameManager.SpawnPlayer())<br>
/// Should be added to the entity list dynamically.<br>
/// </summary>
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
    public Entity CurrentFriendlyTarget { 
        get {
            if(FriendlyTargetId != -1)
            {
                var target = ArenaManager.Instance.GetCurrentArena().GetEntity(FriendlyTargetId);
                if(target == null)
                { 
                    FriendlyTargetId = -1;    
                    return null;            
                }
                else 
                {
                    return target;
                }
            }
            else
            {
                return null;
            } 
        }
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
        input.SetMultiplayerAuthority(Int32.Parse(Name), true);
        if(Multiplayer.GetUniqueId() == Int32.Parse(Name))
		{
			IsLocalPlayer = true;
            camera.GetCamera().Current = true;
            PlayerHUD.Instance.activeCamera = camera.GetCamera();
            mover.QueueFree();
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
    public void Reset()
    {
        //Just reset arsenal for now:
        arsenal.ResetArsenal();
    }
}