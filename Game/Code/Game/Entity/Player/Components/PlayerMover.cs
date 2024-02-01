using Godot;
using System;
using System.Diagnostics;

/// <summary>
/// Player mover, reads parts of the playerinput to send Move commands to the entitycontroller.<br/>
/// *ONLY SERVER* <br/>
/// </summary>
public partial class PlayerMover : Node
{	
	[Export]
	private float SPEED = 10f;
	[Export]
	private float JUMP_VELOCITY = 35f;
    [Export]
    private float DASH_POWER = 50f;

    private PlayerEntity player;
    private PlayerInput input;
	private Camera3D camera;

    private Vector3 dashVector = Vector3.Zero;
    private Vector3 jumpVector = Vector3.Zero;
    private Vector3 moveVector = Vector3.Zero;

	public override void _Ready()
	{
        player = (PlayerEntity)GetParent();       
        input = (PlayerInput)GetNode("%Input"); 
        SetPhysicsProcess(GetMultiplayerAuthority() == 1);
        base._Ready();
	}
    public override void _PhysicsProcess(double delta)
	{		        
		//Only Runs on Server       		 
        var controller = player.Controller;   
        if(input.Jumping && controller.IsOnFloor())
		{	
			jumpVector.Y = JUMP_VELOCITY;
            controller.Move(jumpVector);
            player.Arsenal.TryInteruptCast();
            player.Arsenal.TryInteruptChanneling();
		}
		input.Jumping = false;
        if(!controller.IsOnFloor())
        {           
           Vector3 reverse = new Vector3(0f, -MD.Gravity, 0f);
           jumpVector = jumpVector.Lerp(reverse, 0.1f); 
           controller.Move(jumpVector);
           player.Arsenal.TryInteruptCast();
           player.Arsenal.TryInteruptChanneling();
        }       
         if(input.Dashing)
        {
            dashVector = controller.GlobalBasis.Z.Normalized() * DASH_POWER;
        }
        input.Dashing = false;  
        if(dashVector.Length() > 0)
        {
            controller.Move(dashVector);
            dashVector = dashVector.Lerp(Vector3.Zero, 0.1f);
        }
        var playerSpeed = player.Status.GetCurrentSpeed();     
        moveVector.X = controller.IsOnFloor() ? input.Direction.X * playerSpeed : moveVector.X;
		moveVector.Z = controller.IsOnFloor() ? input.Direction.Z * playerSpeed : moveVector.Z;
        controller.Move(moveVector);        

		if(input.Direction.Length() > 0.2f)
		{           
            player.Arsenal.TryInteruptCast();
            player.Arsenal.TryInteruptChanneling();
			Vector2 lookDirection = new Vector2(input.Direction.Z, input.Direction.X);
			controller.Rotate(lookDirection);
		}
	}
}
