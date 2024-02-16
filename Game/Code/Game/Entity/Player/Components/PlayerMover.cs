using System;
using Godot;

namespace Daikon.Game;

public partial class PlayerMover : Node, IEntityMover
{	
	[Export]
	private float JUMP_VELOCITY = 30f;
    [Export]
    private float DASH_POWER = 5f;

    private PlayerEntity player;
    private PlayerInput input;
	private Camera3D camera;

    private Vector3 direction = Vector3.Zero;
    private Vector3 velocity = Vector3.Zero;
    private float verticalVelocity = 0;
    private float fallGravity = 50f;
    private float jumpGravity;
    private float acceleration = 50;

    private float jumpHeight = 1f;
    private float apexDuration = 0.3f;


	public override void _Ready()
	{
        player = (PlayerEntity)GetParent();       
        input = (PlayerInput)GetNode("%Input"); 
        jumpGravity = fallGravity;
        SetPhysicsProcess(GetMultiplayerAuthority() == 1);
        base._Ready();
	}
    public override void _PhysicsProcess(double delta)
	{		        
        Move((float)delta);
        Rotate((float)delta);
	}


    public void Move(float delta)
    {
        var controller = player.Controller;   
        var playerSpeed = player.Status.GetCurrentSpeed();    

        velocity.X = controller.IsOnFloor() ? playerSpeed * input.Direction.X : velocity.X;
        velocity.Z = controller.IsOnFloor() ? playerSpeed * input.Direction.Z : velocity.Z;

        if(input.Jumping && controller.IsOnFloor())
		{	
            velocity.Y = (2 * jumpHeight) / apexDuration;
            jumpGravity = velocity.Y / apexDuration;
            player.Arsenal.TryInteruptCast();
            player.Arsenal.TryInteruptChanneling();
		}
		input.Jumping = false;               
        if(!controller.IsOnFloor())
        {           
            if(velocity.Y >= 0)
            {
                velocity.Y -= jumpGravity * delta;
            }
            else 
            {
                velocity.Y  -= fallGravity * delta;
            }
        }
        direction = controller.IsOnFloor() ? input.Direction : direction;
        controller.Velocity = controller.Velocity.Lerp(velocity, delta * acceleration);        
    }

    public void Rotate(float delta)
    {
        var controller = player.Controller;   
        if(input.Direction.Length() > 0.2f)
		{           
            player.Arsenal.TryInteruptCast();
            player.Arsenal.TryInteruptChanneling();
			Vector2 lookDirection = new Vector2(input.Direction.Z, input.Direction.X);
			controller.Rotate(lookDirection);
		}
    }

    public void Push(float delta)
    {
        throw new NotImplementedException();
    }

    public void Teleport()
    {
        throw new NotImplementedException();
    }
}
