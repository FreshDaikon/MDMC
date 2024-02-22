using Godot;

namespace Daikon.Game;

public partial class BaseMechanic: Node
{
    private TimelineManager manager;

    public override void _Ready()
    {
        manager = GetParent<TimelineManager>();
    }

    public virtual void StartMechanic(){}

    public virtual void UpdateMechanic(){}

    public virtual void ResolveMechanic(){}
  
}