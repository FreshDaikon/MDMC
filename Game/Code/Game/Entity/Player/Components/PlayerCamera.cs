using Godot;

namespace Daikon.Game;

public partial class PlayerCamera : SpringArm3D
{
    [Export]
	private float sensitivity = 100f;
    //Camera Values:    
	private Vector2 camRotation = Vector2.Zero;
    // References:
	private PlayerEntity player;
    private Camera3D camera;

	public override void _Ready()
	{	
		player = (PlayerEntity)GetParent();
        camera = (Camera3D)GetNode("%Camera");
         //This Component is only useful on Local Clients       
        camRotation.Y = -30f;
		camRotation.X = 0f;
		TopLevel = true;
	}
    public Camera3D GetCamera()
    {
        return camera;
    }
	public override void _Process(double delta)
	{
		if(!player.IsLocalPlayer || !StateManager.Instance.HasFocus)
			return;
		bool isZoom = false;
		if (Input.IsActionPressed("Zoom"))
		{
			isZoom = true;
			SpringLength -= Input.GetActionStrength("Camera_Up") - Input.GetActionStrength("Camera_Down") * sensitivity * (float)delta;
		}
		Vector3 direction = Vector3.Zero;
		direction.X = Input.GetActionStrength("Camera_Left") - Input.GetActionStrength("Camera_Right");
		if (!isZoom)
			direction.Y = Input.GetActionStrength("Camera_Up") - Input.GetActionStrength("Camera_Down");
		//Update Camera:
		camRotation.Y += direction.Y * sensitivity * (float)delta;
		camRotation.Y = Mathf.Clamp(camRotation.Y, -55f, 0f);
		camRotation.X += direction.X * sensitivity * (float)delta;
		camRotation.X = Mathf.Wrap(camRotation.X, 0f, 360f);
		RotationDegrees = new Vector3(camRotation.Y, camRotation.X, 0f);
		SpringLength = Mathf.Clamp(SpringLength, 5f, 35f);	
        // Get Players Position:
        var pos = new Vector3(player.Controller.Position.X, 3f, player.Controller.Position.Z);
		Position = pos;//Position.Lerp(pos, 0.1f); 
	}
}
