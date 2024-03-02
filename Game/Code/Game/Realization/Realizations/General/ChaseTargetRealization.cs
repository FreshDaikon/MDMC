using Godot;

namespace Daikon.Game.General;

public partial class ChaseTargetRealization : Realization
{
    private float _lifetime;
    private Vector3 _startPosition;
    private Node3D _target;
    private float _speed;
    private ulong _startTime;
    

    public void SetData(Vector3 startPosition, Node3D target, float lifetime, float speed)
    {
        _lifetime = lifetime;
        _startPosition = startPosition;
        _target = target;
        _speed = speed;
    }

    public override void Spawn()
    {
        ArenaManager.Instance.GetRealizationPool()?.AddChild(this);
        GlobalPosition = _startPosition;
        _startTime = Time.GetTicksMsec();
        animationPlayer.Play("Spawn"); 
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        var lapsed = (Time.GetTicksMsec() - _startTime) / 1000f;
        
        if (_target == null) return;
        
        var lapsedMultiplier = Mathf.Max(1f, lapsed/_lifetime);
        GlobalPosition = GlobalPosition.MoveToward(_target.GlobalPosition, (_speed * lapsedMultiplier * (float)delta));
        var distance = GlobalPosition.DistanceSquaredTo(_target.GlobalPosition);
        
        if (!(distance <= 0.01f)) return;
        if(animationPlayer.CurrentAnimation == "End") return;
        OnEndStart();
        animationPlayer.Play("End");
    }
}