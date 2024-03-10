using Godot;
using Mdmc.Code.Game.Entity.Adversary.TimelineSolver;

namespace Mdmc.Code.Game.Entity.Adversary.Actions;
 
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