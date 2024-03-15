using Godot;

namespace Mdmc.Code.Game.RealizationSystem.RealizerTypes.Boss.Mechanics;

[GlobalClass]
public partial class RectAoeRealizer : BaseRealizer
{
	[Export]
	private Decal _indicator;
	private Tween _indicatorTween;

	public override void Initialize()
	{
		LookAt(ParentRealization.LookatTarget, Vector3.Up);
		_indicator.Size = new Vector3(
			ParentRealization.Size.X,
			1f,
			ParentRealization.Size.Y);
		_indicator.Position = new Vector3(0f, 0f, -(ParentRealization.Size.Y / 2));
	}
}