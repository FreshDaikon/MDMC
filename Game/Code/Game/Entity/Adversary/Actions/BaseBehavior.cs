using Godot;

namespace Daikon.Game;

public partial class BaseBehavior : Node
{
    private TimelineManager manager;
    private bool ShouldProcess = false;

    public TimelineManager Manager { get { return manager; }}

    public override void _Ready()
    {
        manager = GetParent<TimelineManager>();
    }

    public override void _PhysicsProcess(double delta)
    {
        if(ShouldProcess)
        {
            ProcessBehavior();
        }
    }

    public virtual void ProcessBehavior(){}

    public void StartBehavior()
    {
        ShouldProcess = true;
    }

    public void StopBehavior()
    {
        ShouldProcess = false;
    }
}