using Godot;

namespace Mdmc.Code.Game.Realization.Realizations.Boss.Mechanics;

public partial class RectAoeRealization : Realization
{
	[Export]
	private Decal _indicator;
    
	private Vector3 _position;
	private Vector3 _targetPosition;
	private Vector2 _size;
	private Tween _indicatorTween;
	
	public void SetData(Vector3 startPosition, Vector3 target, float lifetime, Vector2 size)
	{
		Lifetime = lifetime;
		_position = startPosition;
		_targetPosition = target;
		_size = size;
	}

	public override void Spawn()
	{
		RealizationManager.Instance.AddRealization(this);
		StartTime = Time.GetTicksMsec();
		GlobalPosition = _position;
		LookAt(_targetPosition, Vector3.Up);
		_indicator.Size = new Vector3(_size.X, 1f, _size.Y);
		_indicator.Position = new Vector3(0f, 0f, -(_size.Y / 2));
	}
}