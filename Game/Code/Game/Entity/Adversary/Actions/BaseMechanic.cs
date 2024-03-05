using Godot;

namespace Daikon.Game;

public partial class BaseMechanic: Node
{
    [Export] protected float _resolveTime = 5; 
    private ulong _startTime;
    private bool _isActive = false;
    
    protected TimelineManager manager;

    public override void _Ready()
    {
        manager = GetParent<TimelineManager>();
    }

    public void InitiateMechanic()
    {
        if(_isActive) return;
        
        _startTime = Time.GetTicksMsec();
        _isActive = true;
        StartMechanic(); 
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if(!_isActive) return;
        var lapsed = (Time.GetTicksMsec() - _startTime) / 1000f;
        if (!(lapsed > _resolveTime)) return;
        ResolveMechanic();
        _isActive = false;
    }

    protected internal virtual void StartMechanic(){}
    
    protected internal virtual void UpdateMechanic(){}

    protected internal virtual void ResolveMechanic(){}
  
}