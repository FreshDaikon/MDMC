using Godot;

namespace Mdmc.Code.Game.Realization.Realizations.Boss.Mechanics;

[GlobalClass]
public partial class CircleAoeRealizer: BaseRealizer
{
    [Export]
    private Decal _indicator;
    
    private float _radius;
    private Tween _indicatorTween;

    public override void Initialize()
    {
        GD.Print("Realizing Circle AOE?");
        _radius = ParentRealization.Radius;
        _indicator.Size = new Vector3(0.01f, 0.01f, 0.01f);
        _indicatorTween = GetTree().CreateTween();        
        _indicatorTween.TweenProperty(_indicator, "size", new Vector3(_radius * 2, _radius * 2, _radius * 2), 0.5f)
            .SetTrans(Tween.TransitionType.Quad);
    }
}