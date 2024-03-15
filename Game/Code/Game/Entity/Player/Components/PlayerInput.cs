using System.Collections.Generic;
using System.Linq;
using Godot;
using Mdmc.Code.Game.Entity.Components;
using Mdmc.Code.System;
using UIManager = Mdmc.Code.Client.UI.UIManager;

namespace Mdmc.Code.Game.Entity.Player.Components;

public partial class PlayerInput : Node
{
    [Export]
    public bool Jumping = false;
    [Export]
    public bool Dashing = false;
    public Vector3 Direction = Vector3.Zero;
    public Vector3 Rotation = Vector3.Zero;
    public Vector3 LastDirection = Vector3.Zero;
    
     // References:
	private PlayerEntity player;
    private PlayerCamera camera;

    [Signal]
    public delegate void ActionButtonPressedEventHandler(int containerSlot, int slot);
    
    public bool isActivator1Pressed = false;
    public bool isActivator2Pressed = false;
    public bool isActivator3Pressed = false;

    // Time stamp , <container, slot> :
    private Dictionary<BufferMessage, ulong> _skillBuffer = new();
    private record BufferMessage()
    {
        public int container { get; init; }
        public int slot { get; init; }
    }
    private ulong _buffer = 300;
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
        if(player.Status.CurrentState == EntityStatus.StatusState.KnockedOut)
            return;
        HandleInputs();
        UpdateSkillBuffer();

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
            HandleSkillActions(MD.ContainerSlot.Main);
        }
        else if(Input.GetActionStrength("Activator1") > 0f && !isActivator3Pressed)
        {
            isActivator1Pressed = true;
            HandleSkillActions(MD.ContainerSlot.Left);
        }
        else if(Input.GetActionStrength("Activator2") > 0f && !isActivator3Pressed)
        {
            isActivator2Pressed = true;
            HandleSkillActions(MD.ContainerSlot.Right);
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

        // PC:
        if(Input.IsActionJustPressed("ChangeTargetPC"))
        {
            if(Input.IsKeyPressed(Key.Ctrl))
            {
                RpcId(1, nameof(RequestFriendlyTargetChange), true);
            }
            else
            {
               RpcId(1, nameof(RequestEnemyTargetChange), true);
            }
        }
    }
    
    private void AddBufferMessage(int containerSlot, int slot)
    {
        var message = new BufferMessage()
        {
            container = (int)containerSlot,
            slot = slot
        };
        if (_skillBuffer.ContainsKey(message))
        {
            _skillBuffer[message] = Time.GetTicksMsec();
        }
        else
        {
            _skillBuffer.Add(message, Time.GetTicksMsec());
        }
    }
    
    private void HandleHotbars()
    {
        //Setup Triggers:
        if(Input.IsActionJustReleased("Hotbar1"))
        {
            if(Input.IsKeyPressed(Key.Shift))
            {
                EmitSignal(SignalName.ActionButtonPressed, (int)MD.ContainerSlot.Right, 0);
                AddBufferMessage((int)MD.ContainerSlot.Right, 0);
            }
            else if(Input.IsKeyPressed(Key.Ctrl))
            {
                EmitSignal(SignalName.ActionButtonPressed, (int)MD.ContainerSlot.Left, 0);
                AddBufferMessage((int)MD.ContainerSlot.Left, 0);
            }
            else
            {
                EmitSignal(SignalName.ActionButtonPressed, (int)MD.ContainerSlot.Main, 0);
                AddBufferMessage((int)MD.ContainerSlot.Main, 0);
            }
        }
        if(Input.IsActionJustReleased("Hotbar2"))
        {
            if(Input.IsKeyPressed(Key.Shift))
            {
                EmitSignal(SignalName.ActionButtonPressed, (int)MD.ContainerSlot.Right, 1);
                AddBufferMessage((int)MD.ContainerSlot.Right, 1);
            }
            else if(Input.IsKeyPressed(Key.Ctrl))
            {
                EmitSignal(SignalName.ActionButtonPressed, (int)MD.ContainerSlot.Left, 1);
                AddBufferMessage((int)MD.ContainerSlot.Left, 1);
            }
            else
            {
                EmitSignal(SignalName.ActionButtonPressed, (int)MD.ContainerSlot.Main, 1);
                AddBufferMessage((int)MD.ContainerSlot.Main, 1);
            }
        }
        if(Input.IsActionJustReleased("Hotbar3"))
        {
            if(Input.IsKeyPressed(Key.Shift))
            {
                EmitSignal(SignalName.ActionButtonPressed, (int)MD.ContainerSlot.Right, 2);
                AddBufferMessage((int)MD.ContainerSlot.Right, 2);
            }
            else if(Input.IsKeyPressed(Key.Ctrl))
            {
                EmitSignal(SignalName.ActionButtonPressed, (int)MD.ContainerSlot.Left, 2);
                AddBufferMessage((int)MD.ContainerSlot.Left, 2);
            }
            else
            {
                EmitSignal(SignalName.ActionButtonPressed, (int)MD.ContainerSlot.Main, 2);
                AddBufferMessage((int)MD.ContainerSlot.Main, 2);
            }
        }
        if(Input.IsActionJustReleased("Hotbar4"))
        {
            if(Input.IsKeyPressed(Key.Shift))
            {
                EmitSignal(SignalName.ActionButtonPressed, (int)MD.ContainerSlot.Right, 3);
                AddBufferMessage((int)MD.ContainerSlot.Right, 3);
            }
            else if(Input.IsKeyPressed(Key.Ctrl))
            {
                EmitSignal(SignalName.ActionButtonPressed, (int)MD.ContainerSlot.Left, 3);
                AddBufferMessage((int)MD.ContainerSlot.Left, 3);
            }
            else
            {
                EmitSignal(SignalName.ActionButtonPressed, (int)MD.ContainerSlot.Main, 3);
                AddBufferMessage((int)MD.ContainerSlot.Main, 3);
            }
        }
    }    
    
    private void HandleSkillActions(MD.ContainerSlot containerSlot)
    {
        //Setup Triggers:
        if(Input.IsActionJustReleased("ActionButton1"))
        {
            EmitSignal(SignalName.ActionButtonPressed, (int)containerSlot, 0);
            AddBufferMessage((int)containerSlot, 0);
        }
        if(Input.IsActionJustReleased("ActionButton2"))
        {
            EmitSignal(SignalName.ActionButtonPressed, (int)containerSlot, 1);
            AddBufferMessage((int)containerSlot, 1);
        }
        if(Input.IsActionJustReleased("ActionButton3"))
        {
            EmitSignal(SignalName.ActionButtonPressed, (int)containerSlot, 2);
            AddBufferMessage((int)containerSlot, 2); 
        }
        if(Input.IsActionJustReleased("ActionButton4"))
        {
            EmitSignal(SignalName.ActionButtonPressed, (int)containerSlot, 3);
            AddBufferMessage((int)containerSlot, 3);
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
        if(LastDirection != Direction)
        {
            RpcId(1, nameof(SyncDirection), Direction);   
            LastDirection = Direction;
        }

    }

    private void UpdateRotation(Vector3 direction)
    {
        var controller = player.Controller;
        Vector2 lookDirection = new Vector2(direction.Z, direction.X);
        Vector3 rotation = new Vector3(0f, lookDirection.Angle(), 0f); 
		controller.Rotation = rotation;
        RpcId(1, nameof(SyncRotation), rotation);    
    }

    private void UpdateSkillBuffer()
    {        
        var current = Time.GetTicksMsec();
        foreach (var buffered in (from buffered in _skillBuffer
                     let lapsed = Time.GetTicksMsec() - buffered.Value
                     where lapsed > _buffer
                     select buffered).ToList())
        {
            
            _skillBuffer.Remove(buffered.Key);
        }
        // Now then:
        foreach (var skill in _skillBuffer.ToList())
        {
            if (!player.Arsenal.CanCast((MD.ContainerSlot)skill.Key.container, skill.Key.slot).SUCCESS) continue;
            RpcId(1, nameof(TryTriggerSkill), skill.Key.container, skill.Key.slot);
            _skillBuffer.Clear();
            break;
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
        if (!Multiplayer.IsServer()) return;
        
        var current = player.CurrentTarget;
        var enemies = Arena.ArenaManager.Instance.GetCurrentArena().GetEnemyEntities();
        if(current == null )
        {
            player.ChangeTarget(enemies.First());
        }
        else
        {
            var currentIndex = enemies.IndexOf((Adversary.AdversaryEntity)current);
            var newIndex = Mathf.Wrap(currentIndex + (direction ? 1 : -1), 0, enemies.Count);
            var newTarget = enemies[newIndex];
            player.ChangeTarget(newTarget);
        } 
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestFriendlyTargetChange(bool direction)
    {
        if (!Multiplayer.IsServer()) return;
        
        var current = player.CurrentFriendlyTarget;
        var friends = Arena.ArenaManager.Instance.GetCurrentArena().GetPlayers();
        if(current == null )
        {
            player.ChangeFriendlyTarget(friends.First());
        }
        else
        {
            var currentIndex = friends.IndexOf((PlayerEntity)current);
            var newIndex = Mathf.Wrap(currentIndex + (direction ? 1 : -1), 0, friends.Count);
            var newFriend = friends[newIndex];
            player.ChangeFriendlyTarget(newFriend);
        }        
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void TryTriggerSkill(int containerSlot, int slot)
    {
        var result = player.Arsenal.TriggerSkill((MD.ContainerSlot)containerSlot, slot);
        GD.Print("Skill Trigger result [ " + "Success : " + result.SUCCESS + " , Reasons : " + result.result);
    }
}