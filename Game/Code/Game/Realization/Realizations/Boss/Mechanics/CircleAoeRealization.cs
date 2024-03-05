using Godot;

namespace Daikon.Game.Realizations.Boss.Mechanics;


public partial class CircleAoeRealization: Realization
{
    [Export]
    private Decal _indicator;
    
    private Vector3 _position;
    private float _radius;
    private Tween _indicatorTween;

    public void SetData(Vector3 position, float radius, float lifetime)
    {
        _radius = radius;
        _position = position;
        Lifetime = lifetime;
    }

    public override void Spawn()
    {
        RealizationManager.Instance?.AddRealization(this);
        StartTime = Time.GetTicksMsec();
        GlobalPosition = _position;
        _indicator.Size = new Vector3(0.01f, 0.01f, 0.01f);
        _indicatorTween = GetTree().CreateTween();
        
        _indicatorTween.TweenProperty(_indicator, "size", new Vector3(_radius * 2, _radius * 2, _radius * 2), 0.5f)
            .SetTrans(Tween.TransitionType.Quad);
    }
}