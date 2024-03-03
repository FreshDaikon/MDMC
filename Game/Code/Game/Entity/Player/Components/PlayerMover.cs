using System;
using Godot;

namespace Daikon.Game;

public partial class PlayerMover : Node, IEntityMover
{	
	[Export]
	private float _jumpVelocity = 30f;
    [Export]
    private float _dashPower = 5f;

    private PlayerEntity player;
    private PlayerInput input;
	private Camera3D camera;

    private Vector3 _direction = Vector3.Zero;
    private Vector3 _velocity = Vector3.Zero;
    private float _verticalVelocity = 0;
    private float _fallGravity = 50f;
    private float _jumpGravity;

    private float jumpHeight = 1f;
    private float apexDuration = 0.3f;

	public override void _Ready()
	{
        player = (PlayerEntity)GetParent();       
        input = (PlayerInput)GetNode("%Input"); 
        _jumpGravity = _fallGravity;
        base._Ready();
	}
    public override void _PhysicsProcess(double delta)
	{
        if (player.Status.CurrentState == EntityStatus.StatusState.KnockedOut)
        {
            player.Controller.Velocity = Vector3.Zero;
            return;
        }
        
        Move((float)delta);
        Rotate((float)delta);
	}

    public void Move(float delta)
    {
        var controller = player.Controller;   
        var playerSpeed = player.Status.CurrentSpeed;

        _velocity.X = controller.IsOnFloor() ? playerSpeed * input.Direction.X : _velocity.X;
        _velocity.Z = controller.IsOnFloor() ? playerSpeed * input.Direction.Z : _velocity.Z;

        if(input.Jumping && controller.IsOnFloor())
		{	
            _velocity.Y = (2 * jumpHeight) / apexDuration;
            _jumpGravity = _velocity.Y / apexDuration;
            if(player.Arsenal.IsCasting || player.Arsenal.IsChanneling)
            {
                if(player.Arsenal.ChannelingSkill != null) player.Arsenal.TryInterruptChanneling();
            }  
		}
		input.Jumping = false;               
        if(!controller.IsOnFloor())
        {           
            if(_velocity.Y >= 0)
            {
                _velocity.Y -= _jumpGravity * delta;
            }
            else 
            {
                _velocity.Y  -= _fallGravity * delta;
            }
        }
        _direction = controller.IsOnFloor() ? input.Direction : _direction;
        if(_direction.Length() > 0f && (player.Arsenal.IsCasting || player.Arsenal.IsChanneling))
        {
           if(player.Arsenal.ChannelingSkill != null) player.Arsenal.TryInterruptChanneling();
        }
        controller.Velocity = _velocity; //controller.Velocity.Lerp(velocity, delta * acceleration);        
    }

    public void Rotate(float delta)
    {
        if(Multiplayer.IsServer())
        {
            player.Controller.Rotation = input.Rotation;
        }
    }

    public void Push(float delta)
    {
        GD.Print("Push!!!");
    }

    public void Teleport(Vector3 position)
    {
        GD.Print("Teleporting player!");
        player.Controller.Position = position;
    }
}
