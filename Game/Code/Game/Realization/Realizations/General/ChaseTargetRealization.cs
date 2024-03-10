using Godot;

namespace Mdmc.Code.Game.Realization.Realizations.General;

public partial class ChaseTargetRealization : Realization
{
    [Export]
    protected AnimationPlayer AnimationPlayer;
    
    private Vector3 _startPosition;
    private Node3D _target;
    private float _speed;

    public void SetData(Vector3 startPosition, Node3D target, float lifetime, float speed)
    {
        Lifetime = lifetime;
        _startPosition = startPosition;
        _target = target;
        _speed = speed;
    }

    public override void Spawn()
    {
        RealizationManager.Instance?.AddRealization(this);
        GlobalPosition = _startPosition;
        StartTime = Time.GetTicksMsec();
        AnimationPlayer.Play("Spawn"); 
    }

    public override void _PhysicsProcess(double delta)
    {
        var lapsed = (Time.GetTicksMsec() - StartTime) / 1000f;
        
        if (_target == null) return;
        
        var lapsedMultiplier = Mathf.Max(1f, lapsed/Lifetime);
        GlobalPosition = GlobalPosition.MoveToward(_target.GlobalPosition, (_speed * lapsedMultiplier * (float)delta));
        var distance = GlobalPosition.DistanceSquaredTo(_target.GlobalPosition);
        
        if (!(distance <= 0.01f)) return;
        EmitSignal(SignalName.OnRealizationEnd);
        Despawn();
    }
}