using Godot;
using UIManager = Mdmc.Code.Client.UI.UIManager;

namespace Mdmc.Code.Game.Entity.Player.Components;

public partial class PlayerCamera : SpringArm3D
{
    [Export]
	private float sensitivity = 100f;
	[Export]
	private float mouseSensitivity = 0.3f;
    //Camera Values:    
    // References:
	private PlayerEntity player;
    private Camera3D camera;

	private bool _isLookOn;
	private Vector2 _desiredRotation = Vector2.Zero;
	private Vector2 _direction = Vector2.Zero;
	private bool _isPC = false;
	private Vector2 _mouseMotion = Vector2.Zero;
	private Vector2 _mousePosition = Vector2.Zero;
	private Vector2 _storedPosition = Vector2.Zero;
	private bool _firstPress = true;

	public override void _Ready()
	{	
		player = (PlayerEntity)GetParent();
        camera = (Camera3D)GetNode("%Camera");
         //This Component is only useful on Local Clients       
        _desiredRotation.Y = -30f;
		_desiredRotation.X = 0f;
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
		if(UIManager.Instance.GetCurrentState() != UIManager.UIState.HUD)
            return;
		// Controller Control:
		bool isZoom = false;
		if (Input.IsActionPressed("Zoom"))
		{
			isZoom = true;
			SpringLength -= Input.GetActionStrength("Camera_Up") - Input.GetActionStrength("Camera_Down") * sensitivity * (float)delta;
		}
		_direction.X = Input.GetActionStrength("Camera_Left") - Input.GetActionStrength("Camera_Right");
		if (!isZoom)
			_direction.Y = Input.GetActionStrength("Camera_Up") - Input.GetActionStrength("Camera_Down");
		//Update Camera:
		_desiredRotation.Y += _direction.Y * sensitivity * (float)delta;
		_desiredRotation.Y = Mathf.Clamp(_desiredRotation.Y, -90f, 0f);
		_desiredRotation.X += _direction.X * sensitivity * (float)delta;
		_desiredRotation.X = Mathf.Wrap(_desiredRotation.X, 0f, 360f);

		if(Input.IsActionPressed("MouseLook") || Input.IsActionPressed("MousePress"))
		{
			if(_firstPress)
			{
				_storedPosition = _mousePosition;
				Input.MouseMode = Input.MouseModeEnum.Captured;
				_firstPress = false;
			}
			_direction.X = -_mouseMotion.X * mouseSensitivity;
			_direction.Y = -_mouseMotion.Y * mouseSensitivity;
			_desiredRotation.Y += _direction.Y;
			_desiredRotation.Y = Mathf.Clamp(_desiredRotation.Y, -90f, 0f);
			_desiredRotation.X += _direction.X;
			_desiredRotation.X = Mathf.Wrap(_desiredRotation.X, 0f, 360f);
			_mouseMotion = Vector2.Zero;
		}
		else
		{
			if(!_firstPress)
			{
				_firstPress = true;
				Input.MouseMode = Input.MouseModeEnum.Visible;
				Input.WarpMouse(_storedPosition);
			}
		}
		RotationDegrees = new Vector3(_desiredRotation.Y, _desiredRotation.X, 0f);
		SpringLength = Mathf.Clamp(SpringLength, 5f, 35f);	
		var pos = new Vector3(player.Controller.Position.X, player.Controller.Position.Y + 2f, player.Controller.Position.Z);
		Position = pos;//Position.Lerp(pos, 0.1f); 
		camera.Fov = Mathf.Remap(SpringLength, 5f, 35f, 75f, 65f);
		_direction = Vector2.Zero;
		
	}

    public override void _Input(InputEvent @event)
    {
		if(@event is InputEventMouseMotion motionEvent)
		{ 
			_mousePosition = motionEvent.GlobalPosition;
			_mouseMotion = motionEvent.Relative;
		}
		if(@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
		{
			var current = SpringLength;
			switch(mouseEvent.ButtonIndex)
			{
				case MouseButton.WheelUp:
					current = Mathf.Clamp(current - 1f, 5f, 35f);
					break;
				case MouseButton.WheelDown:
					current = Mathf.Clamp(current + 1f, 5f, 35f);
					break;
			}
			SpringLength = current;
		}
		Mathf.Clamp(SpringLength, 5f, 35f);
    }
}
