using Godot;

namespace Daikon.Game;
 
public partial class BaseTriggerCheck: Node
{
    private TimelineManager manager;

    public override void _Ready()
    {
        manager = GetParent<TimelineManager>();
    }

    public virtual void CheckTrigger(){ }

    public virtual void ResolveTrigger(){ }

}