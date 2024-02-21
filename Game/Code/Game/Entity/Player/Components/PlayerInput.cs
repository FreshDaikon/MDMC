using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class PlayerInput : Node
{
    [Export]
    public bool Jumping = false;
    [Export]
    public bool Dashing = false;
    [Export]
    public Vector3 Direction = Vector3.Zero;
    [Export]
    public Vector3 Rotation = Vector3.Zero;
    
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
        if(Input.IsKeyPressed(Key.F2))
        {
            //Let's Reset everything
            RpcId(1, nameof(RequestReset));
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
            RpcId(1, nameof(RequestFriendlyTargetChange), true);
        }
        if(Input.IsActionJustPressed("SelectFriendlyUp"))
        {
            RpcId(1, nameof(RequestFriendlyTargetChange), false);
        }
        if(Input.IsActionJustPressed("SelectTargetDown"))
        {
            RpcId(1, nameof(RequestEnemyTargetChange), true);
        }
        if(Input.IsActionJustPressed("SelectTargetUp"))
        {
            RpcId(1, nameof(RequestEnemyTargetChange), false);
        }
    }
    private void HandleSkillActions(string ContainerName)
    {
        if(player.Arsenal.CanCast(ContainerName, 0).SUCCESS)
        {            
            if(InputBuffer.Instance.IsActionPressedBuffered("ActionButton1", 350f))
            {
                RpcId(1, nameof(TryTriggerSkill), ContainerName, 0);
            }
        }
        if(player.Arsenal.CanCast(ContainerName, 1).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("ActionButton2", 350f))
            {
                RpcId(1, nameof(TryTriggerSkill), ContainerName, 1);
            }
        }
        if(player.Arsenal.CanCast(ContainerName, 2).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("ActionButton3", 350f))
            {
                RpcId(1, nameof(TryTriggerSkill), ContainerName, 2);
            }
        }
        if(player.Arsenal.CanCast(ContainerName, 3).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("ActionButton4", 350f))
            {
                RpcId(1, nameof(TryTriggerSkill), ContainerName, 3);
            }
        }
    }

    private void HandleMovement()
    {
        Direction = Vector3.Zero;
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
        
        if(!Input.IsActionPressed("MousePress") && Input.IsActionPressed("MouseLook") && Direction.Length() <= 0f)
        {
            var camBasis = camera.GlobalTransform.Basis;
            var basis = camBasis.Rotated(camBasis.X, -camBasis.GetEuler().X);
            var camDir = basis * new Vector3(0f, 0f, -1f);
            UpdateRotation(camDir);
        }
        else if(Input.IsActionPressed("MousePress") && Input.IsActionPressed("MouseLook") )
        {
            Direction = new Vector3(0f, 0f, -1f)
            {
                X = Input.GetActionStrength("Move_Right") - Input.GetActionStrength("Move_Left")
            };
        }
        if(Direction.Length() > 0f)
        {
            var camBasis = camera.GlobalTransform.Basis;
            var basis = camBasis.Rotated(camBasis.X, -camBasis.GetEuler().X);
            Direction = basis * Direction;
            if(Direction.LengthSquared() > 1.0f )
            {
                Direction = Direction.Normalized();
            }
            UpdateRotation(Direction);
        }        
    }

    private void UpdateRotation(Vector3 direction)
    {
        var controller = player.Controller;
        Vector2 lookDirection = new Vector2(direction.Z, direction.X);
        Vector3 rotation = new Vector3(0f, lookDirection.Angle(), 0f); 
		controller.Rotation = rotation;
        RpcId(1, nameof(SetRotation), rotation);    
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

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    private void SetRotation(Vector3 rotation)
    {
        if(Multiplayer.IsServer())
        {
            Rotation = rotation;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestEnemyTargetChange(bool direction)
    {
        if(Multiplayer.IsServer())
        {
            if(player.TargetId == -1)
            {
                var enemies = ArenaManager.Instance.GetCurrentArena().GetEnemyEntities();
                if(enemies == null)
                {
                    player.TargetId = -1;
                    return;
                }
                var target  = enemies[0];
                player.TargetId = int.Parse(target.Name);                
            }
            else
            {                
                var enemies = ArenaManager.Instance.GetCurrentArena().GetEnemyEntities();
                if(enemies == null )
                {
                    player.TargetId = -1;
                    return;
                }
                int index = ArenaManager.Instance.GetCurrentArena().GetEnemyIndex(player.TargetId) + (direction ? 1 : -1);
                int newIndex = Mathf.Wrap(index, 0, enemies.Count);
                player.TargetId = int.Parse(enemies[newIndex].Name);
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
                var friends = ArenaManager.Instance.GetCurrentArena().GetFriendlyEntities();
                if(friends == null)
                {
                    player.FriendlyTargetId = -1;
                    return;
                }
                var target = friends[0];
                player.FriendlyTargetId = int.Parse(target.Name);                
            }
            else
            {                
                var friends = ArenaManager.Instance.GetCurrentArena().GetFriendlyEntities();
                if(friends == null)
                {
                    player.FriendlyTargetId = -1;
                    return;
                }
                int index = ArenaManager.Instance.GetCurrentArena().GetFriendlyIndex(player.FriendlyTargetId) + (direction ? 1 : -1);
                int newIndex = Mathf.Wrap(index, 0, friends.Count);
                player.FriendlyTargetId = int.Parse(friends[newIndex].Name);
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