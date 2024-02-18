using Godot;
using Daikon.Game;

namespace Daikon.Client;

public partial class DamageNumber : Control
{	
	[Export]
	public Label label;
	private Vector2 UIPos;
	private Camera3D camera;
	private Vector3 _worldPos;

	[Export]
	private AnimationPlayer animationPlayer;
	
	public void Initialize(string value, Color color, Vector3 worldPos)
	{
		var ran = new RandomNumberGenerator();
		_worldPos = worldPos + new Vector3(ran.RandfRange(-0.6f, 0.6f), ran.RandfRange(-0.6f, 0.6f), ran.RandfRange(-0.6f, 0.6f));
		camera = PlayerHUD.Instance.activeCamera;
		UIPos = camera.UnprojectPosition(_worldPos);
		label.Text = value;
		label.AddThemeColorOverride("font_color", color);
		Position = UIPos;
		//Play the animation :
		animationPlayer.Play("FloatAway");
		//On End, just delete yourself:
		animationPlayer.AnimationFinished += (animation) => QueueFree();
	}
    public override void _PhysicsProcess(double delta)
    {
		if(ClientMultiplayerManager.Instance.GetStatus() != MultiplayerPeer.ConnectionStatus.Connected)
			return;
		if(camera.IsPositionBehind(_worldPos))
			return;
		UIPos = camera.UnprojectPosition(_worldPos);
		Position = UIPos; // + new Vector2(ran.RandfRange(-100f, 100f), ran.RandfRange(-100f, 100f));
        base._PhysicsProcess(delta);
    }
}
