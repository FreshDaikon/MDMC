using Godot;
using Daikon.Helpers;
using Daikon.Client;

namespace Daikon.Game;

public partial class PlayerInput : Node
{
    [Export]
    public bool Jumping = false;
    [Export]
    public bool Dashing = false;
    public Vector3 Direction = Vector3.Zero;
    public Vector3 Rotation = Vector3.Zero;
    
     // References:
	private PlayerEntity player;
    private PlayerCamera camera;

    [Signal]
    public delegate void ActionButtonPressedEventHandler(string containerName, int slot);
    
    public bool isActivator1Pressed = false;
    public bool isActivator2Pressed = false;
    public bool isActivator3Pressed = false;

    public override void _Ready()
    {
        player = (PlayerEntity)GetParent();
        camera = player.GetNode<PlayerCamera>("%Rig");       
    }
    public override void _Process(double delta)
    {
        if(GetMultiplayerAuthority() != Multiplayer.GetUniqueId())
            return;
        if(!StateManager.Instance.HasFocus)
            return;
        if(UIManager.Instance.GetCurrentState() != UIManager.UIState.HUD)
            return;
        HandleInputs();
    }    

    private bool Activators()
    {
        return isActivator1Pressed || isActivator2Pressed || isActivator3Pressed;
    }
    private void HandleInputs()
    { 
        isActivator1Pressed = false;
        isActivator2Pressed = false;
        isActivator3Pressed = false;
        HandleSelection();
        HandleHotbars();
        if(Input.GetActionStrength("Activator1") > 0f && Input.GetActionStrength("Activator2") > 0f )
        {
            isActivator3Pressed = true;
            HandleSkillActions(PlayerArsenal.ContainerNames.Main);
        }
        else if(Input.GetActionStrength("Activator1") > 0f && !isActivator3Pressed)
        {
            isActivator1Pressed = true;
            HandleSkillActions(PlayerArsenal.ContainerNames.Left);
        }
        else if(Input.GetActionStrength("Activator2") > 0f && !isActivator3Pressed)
        {
            isActivator2Pressed = true;
            HandleSkillActions(PlayerArsenal.ContainerNames.Right);
        }
        HandleMovement();
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
    private void HandleHotbars()
    {
        //Setup Triggers:
        if(Input.IsActionJustPressed("Hotbar1"))
        {
            if(Input.IsKeyPressed(Key.Shift))
                EmitSignal(SignalName.ActionButtonPressed, PlayerArsenal.ContainerNames.Right, 0);
            else if(Input.IsKeyPressed(Key.Ctrl))
                EmitSignal(SignalName.ActionButtonPressed, PlayerArsenal.ContainerNames.Left, 0);
            else
                EmitSignal(SignalName.ActionButtonPressed, PlayerArsenal.ContainerNames.Main, 0);
        }
        if(Input.IsActionJustPressed("Hotbar2"))
        {
            if(Input.IsKeyPressed(Key.Shift))
                EmitSignal(SignalName.ActionButtonPressed, PlayerArsenal.ContainerNames.Right, 1);
            else if(Input.IsKeyPressed(Key.Ctrl))
                EmitSignal(SignalName.ActionButtonPressed, PlayerArsenal.ContainerNames.Left, 1);
            else
                EmitSignal(SignalName.ActionButtonPressed, PlayerArsenal.ContainerNames.Main, 1);
        }
        if(Input.IsActionJustPressed("Hotbar3"))
        {
            if(Input.IsKeyPressed(Key.Shift))
                EmitSignal(SignalName.ActionButtonPressed, PlayerArsenal.ContainerNames.Right, 2);
            else if(Input.IsKeyPressed(Key.Ctrl))
                EmitSignal(SignalName.ActionButtonPressed, PlayerArsenal.ContainerNames.Left, 2);
            else
                EmitSignal(SignalName.ActionButtonPressed, PlayerArsenal.ContainerNames.Main, 2);
        }
        if(Input.IsActionJustPressed("Hotbar4"))
        {
            if(Input.IsKeyPressed(Key.Shift))
                EmitSignal(SignalName.ActionButtonPressed, PlayerArsenal.ContainerNames.Right, 3);
            else if(Input.IsKeyPressed(Key.Ctrl))
                EmitSignal(SignalName.ActionButtonPressed, PlayerArsenal.ContainerNames.Left, 3);
            else
                EmitSignal(SignalName.ActionButtonPressed, PlayerArsenal.ContainerNames.Main, 3);
        }

        if(player.Arsenal.CanCast(PlayerArsenal.ContainerNames.Main, 0).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("Hotbar1"))
            {
                RpcId(1, nameof(TryTriggerSkill), PlayerArsenal.ContainerNames.Main, 0);
            }
        }
        if(player.Arsenal.CanCast(PlayerArsenal.ContainerNames.Main, 1).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("Hotbar2"))
            {
                RpcId(1, nameof(TryTriggerSkill), PlayerArsenal.ContainerNames.Main, 1);
            }
        }
        if(player.Arsenal.CanCast(PlayerArsenal.ContainerNames.Main, 2).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("Hotbar3"))
            {
                RpcId(1, nameof(TryTriggerSkill), PlayerArsenal.ContainerNames.Main, 2);
            }
        }
        if(player.Arsenal.CanCast(PlayerArsenal.ContainerNames.Main, 3).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("Hotbar4"))
            {
                RpcId(1, nameof(TryTriggerSkill), PlayerArsenal.ContainerNames.Main, 3);
            }
        }
        if(player.Arsenal.CanCast(PlayerArsenal.ContainerNames.Right, 0).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("Hotbar5"))
            {
                RpcId(1, nameof(TryTriggerSkill), PlayerArsenal.ContainerNames.Right, 0);
            }
        }
        if(player.Arsenal.CanCast(PlayerArsenal.ContainerNames.Right, 1).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("Hotbar6"))
            {
                RpcId(1, nameof(TryTriggerSkill), PlayerArsenal.ContainerNames.Right, 1);
            }
        }
        if(player.Arsenal.CanCast(PlayerArsenal.ContainerNames.Right, 2).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("Hotbar7"))
            {
                RpcId(1, nameof(TryTriggerSkill), PlayerArsenal.ContainerNames.Right, 2);
            }
        }
        if(player.Arsenal.CanCast(PlayerArsenal.ContainerNames.Right, 3).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("Hotbar8"))
            {
                RpcId(1, nameof(TryTriggerSkill), PlayerArsenal.ContainerNames.Right, 3);
            }
        }
        if(player.Arsenal.CanCast(PlayerArsenal.ContainerNames.Left, 0).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("Hotbar9"))
            {
                RpcId(1, nameof(TryTriggerSkill), PlayerArsenal.ContainerNames.Left, 0);
            }
        }
        if(player.Arsenal.CanCast(PlayerArsenal.ContainerNames.Left, 1).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("Hotbar10"))
            {
                RpcId(1, nameof(TryTriggerSkill), PlayerArsenal.ContainerNames.Left, 1);
            }
        }
        if(player.Arsenal.CanCast(PlayerArsenal.ContainerNames.Left, 2).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("Hotbar11"))
            {
                RpcId(1, nameof(TryTriggerSkill), PlayerArsenal.ContainerNames.Left, 2);
            }
        }
        if(player.Arsenal.CanCast(PlayerArsenal.ContainerNames.Left, 3).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("Hotbar12"))
            {
                RpcId(1, nameof(TryTriggerSkill), PlayerArsenal.ContainerNames.Left, 3);
            }
        }
    }


    private void HandleSkillActions(string ContainerName)
    {
        //Setup Triggers:
        if(Input.IsActionPressed("ActionButton1"))
        {
            EmitSignal(SignalName.ActionButtonPressed, ContainerName, 0);
        }
        if(Input.IsActionPressed("ActionButton2"))
        {
            EmitSignal(SignalName.ActionButtonPressed, ContainerName, 1);
        }
        if(Input.IsActionPressed("ActionButton3"))
        {
            EmitSignal(SignalName.ActionButtonPressed, ContainerName, 2);
        }
        if(Input.IsActionPressed("ActionButton4"))
        {
            EmitSignal(SignalName.ActionButtonPressed, ContainerName, 3);
        }
        if(player.Arsenal.CanCast(ContainerName, 0).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("ActionButton1"))
            {
                RpcId(1, nameof(TryTriggerSkill), ContainerName, 0);
            }
        }
        if(player.Arsenal.CanCast(ContainerName, 1).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("ActionButton2"))
            {
                RpcId(1, nameof(TryTriggerSkill), ContainerName, 1);
            }
        }            
        if(player.Arsenal.CanCast(ContainerName, 2).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("ActionButton3"))
            {
                RpcId(1, nameof(TryTriggerSkill), ContainerName, 2);
            }
        }
        if(player.Arsenal.CanCast(ContainerName, 3).SUCCESS)
        {
            if(InputBuffer.Instance.IsActionPressedBuffered("ActionButton4"))
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
        RpcId(1, nameof(SyncDirection), Direction);   
    }

    private void UpdateRotation(Vector3 direction)
    {
        var controller = player.Controller;
        Vector2 lookDirection = new Vector2(direction.Z, direction.X);
        Vector3 rotation = new Vector3(0f, lookDirection.Angle(), 0f); 
		controller.Rotation = rotation;
        RpcId(1, nameof(SyncRotation), rotation);    
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
    private void SyncDirection(Vector3 direction)
    {
        if(Multiplayer.IsServer())
        {
            Direction = direction;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    private void SyncRotation(Vector3 rotation)
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