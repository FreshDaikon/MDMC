using Godot;
using GameManager = Mdmc.Code.Game.GameManager;

namespace Mdmc.Code.Client.UI.HUD.Misc;

public partial class DamageNumber : Control
{	
	[Export] private Label _label;
	[Export] private AnimationPlayer _animationPlayer;
	
	private Vector2 _screenPosition;
	private Camera3D _camera;
	private Vector3 _worldPos;
	
	public void Initialize(string value, Color color, Vector3 worldPos)
	{
		var ran = new RandomNumberGenerator();
		_worldPos = worldPos + new Vector3(ran.RandfRange(-1f, 1f), ran.RandfRange(-1f, 1f), ran.RandfRange(-1f, 1f));
		_camera = GetViewport().GetCamera3D();
		_screenPosition = _camera.UnprojectPosition(_worldPos);
		_label.Text = value;
		_label.AddThemeColorOverride("font_color", color);
		Position = _screenPosition;
		_animationPlayer.Play("FloatAway");
		_animationPlayer.AnimationFinished += (animation) => QueueFree();
	}
    public override void _PhysicsProcess(double delta)
    {
		if(!GameManager.Instance.IsGameRunning())
			return;
		if(_camera == null)
		{
				return;
		}
		if(_camera.IsPositionBehind(_worldPos))
			return;
			
		_screenPosition = _camera.UnprojectPosition(_worldPos);
		Position = _screenPosition; 
    }
}
