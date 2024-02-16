using Godot;
using Daikon.System;

namespace Daikon.Game;

public partial class PlayerInput : Node
{
    [Export]
    public bool Jumping = false;
    [Export]
    public bool Dashing = false;
    [Export]
    public Vector3 Direction = Vector3.Zero;
    
     // References:
	private PlayerEntity player;
    private PlayerCamera camera;
    private MultiplayerSynchronizer synchronizer;

    [Signal]
    public delegate void ActivatorPressedEventHandler(string containerName);
    [Signal]
    public delegate void ActivatorDepressedEventHandler(string containerName);
    [Signal]
    public delegate void ActionButtonPressedEventHandler(string containerName, int slot);

    public override void _Ready()
    {
        player = (PlayerEntity)GetParent();
        camera = player.GetNode<PlayerCamera>("%Rig");       
        synchronizer = GetNode<MultiplayerSynchronizer>("%Sync");  
    }
    public override void _Process(double delta)
    {
        if(GetMultiplayerAuthority() != Multiplayer.GetUniqueId())
            return;
        if(!StateManager.Instance.HasFocus)
            return;

        HandleInputs();

        //Handle Debug:
        if(Input.IsKeyPressed(Key.F2))
        {
            //Let's Reset everything
            Rpc(nameof(RequestReset));
        }
    }

    private bool isActivator1Pressed = false;
    private bool isActivator2Pressed = false;
    private bool isActivator3Pressed = false;

    private bool Activators()
    {
        return isActivator1Pressed || isActivator2Pressed || isActivator3Pressed;
    }

    private void HandleInputs()
    {
        HandleSelection();
        HandleMovement();
        if(Input.GetActionStrength("Activator1") > 0f && Input.GetActionStrength("Activator2") > 0f )
        {
            if(!isActivator3Pressed)
            {
                EmitSignal(nameof(ActivatorPressed), PlayerArsenal.ContainerNames.Main);
            }
            isActivator3Pressed = true;
            //Handle Main:
            HandleSkillActions(PlayerArsenal.ContainerNames.Main);
        }
        else
        {
            if(isActivator3Pressed)
            {
                EmitSignal(nameof(ActivatorDepressed), PlayerArsenal.ContainerNames.Main);
                isActivator3Pressed = false;
            }
        }
        if(Input.GetActionStrength("Activator1") > 0f && !isActivator3Pressed)
        {
            if(!isActivator1Pressed)
            {
                EmitSignal(nameof(ActivatorPressed), PlayerArsenal.ContainerNames.Left);
            }
            isActivator1Pressed = true;
            //Handle Left:
            HandleSkillActions(PlayerArsenal.ContainerNames.Left);
        }
        else
        {
            if(isActivator1Pressed)
            {
                EmitSignal(nameof(ActivatorDepressed), PlayerArsenal.ContainerNames.Left);
                isActivator1Pressed = false;
            }
        }
        if(Input.GetActionStrength("Activator2") > 0f && !isActivator3Pressed)
        {
            if(!isActivator2Pressed)
            {
                EmitSignal(nameof(ActivatorPressed), PlayerArsenal.ContainerNames.Right);
            }
            isActivator2Pressed = true;
            //Handle Right:
            HandleSkillActions(PlayerArsenal.ContainerNames.Right);
        }
        else
        {
            if(isActivator2Pressed)
            {
                EmitSignal(nameof(ActivatorDepressed), PlayerArsenal.ContainerNames.Right);
                isActivator2Pressed = false;
            }
        }     
    }

    private void HandleSelection()
    {
        if(Input.IsActionJustPressed("SelectFriendlyDown"))
        {
            Rpc(nameof(RequestFriendlyTargetChange), true);
        }
        if(Input.IsActionJustPressed("SelectFriendlyUp"))
        {
            Rpc(nameof(RequestFriendlyTargetChange), false);
        }
        if(Input.IsActionJustPressed("SelectTargetDown"))
        {
            Rpc(nameof(RequestEnemyTargetChange), true);
        }
        if(Input.IsActionJustPressed("SelectTargetUp"))
        {
            Rpc(nameof(RequestEnemyTargetChange), false);
        }
    }
    private void HandleSkillActions(string ContainerName)
    {
        if(player.Arsenal.CanCast(ContainerName, 0).SUCCESS)
        {            
            if(InputBuffer.Instance.IsActionPressedBuffered("ActionButton1", 350f))
            {
                Rpc(nameof(TryTriggerSkill), ContainerName, 0);
            }
        }
        if(player.Arsenal.CanCast(ContainerName, 1).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("ActionButton2", 350f))
            {
                Rpc(nameof(TryTriggerSkill), ContainerName, 1);
            }
        }
        if(player.Arsenal.CanCast(ContainerName, 2).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("ActionButton3", 350f))
            {
                Rpc(nameof(TryTriggerSkill), ContainerName, 2);
            }
        }
        if(player.Arsenal.CanCast(ContainerName, 3).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("ActionButton4", 350f))
            {
                Rpc(nameof(TryTriggerSkill), ContainerName, 3);
            }
        }
    }

    private void HandleMovement()
    {
        if(Input.IsActionJustPressed("Jump") && Activators() != true)
        {
            Rpc(nameof(Jump));
        }     
        if(Input.IsActionJustPressed("Dash") && Activators() != true)
        {
            Rpc(nameof(Dash));
        }  
        Direction.X = Input.GetActionStrength("Move_Right") - Input.GetActionStrength("Move_Left");
        Direction.Z = Input.GetActionStrength("Move_Down") - Input.GetActionStrength("Move_Up");
        var camBasis = camera.GlobalTransform.Basis;
        var basis = camBasis.Rotated(camBasis.X, -camBasis.GetEuler().X);
        Direction = basis * Direction;
        if(Direction.LengthSquared() > 1.0f )
        {
            Direction = Direction.Normalized();
        }
    }
 
    //RPC Calls: 
    [Rpc(CallLocal = true)]
    public void Jump()
    {        
        Jumping = true;
    }
    [Rpc(CallLocal = true)]
    public void Dash()
    {        
        Dashing = true;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestEnemyTargetChange(bool direction)
    {
        if(Multiplayer.IsServer())
        {
            if(player.TargetId == -1)
            {
                var target = ArenaManager.Instance.GetCurrentArena().GetEnemyEntities()[0];
                if(target != null)
                {
                    player.TargetId = int.Parse(target.Name);
                }
                else
                {
                    player.TargetId = -1;
                }
            }
            else
            {                
                int index = ArenaManager.Instance.GetCurrentArena().GetEnemyIndex(player.TargetId) + (direction ? 1 : -1);
                int newIndex = Mathf.Wrap(index, 0, ArenaManager.Instance.GetCurrentArena().GetEnemyEntities().Count);
                player.TargetId = int.Parse(ArenaManager.Instance.GetCurrentArena().GetEnemyEntities()[newIndex].Name);
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestReset()
    {
        if(Multiplayer.IsServer())
        {
            player.Reset();
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestFriendlyTargetChange(bool direction)
    {
        if(Multiplayer.IsServer())
        {
            if(player.FriendlyTargetId == -1)
            {
                var target = ArenaManager.Instance.GetCurrentArena().GetFriendlyEntities()[0];
                if(target != null)
                {
                    player.FriendlyTargetId = int.Parse(target.Name);
                }
                else
                {
                    player.FriendlyTargetId = -1;
                }
            }
            else
            {                
                int index = ArenaManager.Instance.GetCurrentArena().GetFriendlyIndex(player.FriendlyTargetId) + (direction ? 1 : -1);
                int newIndex = Mathf.Wrap(index, 0, ArenaManager.Instance.GetCurrentArena().GetFriendlyEntities().Count);
                player.FriendlyTargetId = int.Parse(ArenaManager.Instance.GetCurrentArena().GetFriendlyEntities()[newIndex].Name);
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void TryTriggerSkill(string containerName, int slot)
    {
        var result = player.Arsenal.TriggerSkill(containerName, slot);
        MD.Log("Skill Trigger result [ " + "Success : " + result.SUCCESS + " , Reasons : " + result.result);
    }
}