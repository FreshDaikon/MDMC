using Godot;
using Mdmc.Code.Game.Entity.Adversary.TimelineSolver;

namespace Mdmc.Code.Game.Entity.Adversary.Actions;

public partial class BaseBehavior : Node
{
    private TimelineManager manager;
    private bool ShouldProcess = false;

    public TimelineManager Manager { get { return manager; }}
    public AdversaryEntity Entity;

    public override void _Ready()
    {
        manager = GetParent<TimelineManager>();
        Entity = manager.Entity;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(!Multiplayer.IsServer()) 
            return;
        if(ShouldProcess)
        {
            if(!CheckBehaviorState())
            {
                StopBehavior();
                return;
            }
            ProcessBehavior();
        }
    }

    public virtual void ProcessBehavior(){}
    public virtual void OnStart(){}
    public virtual void OnEnd(){}

    public void StartBehavior()
    {
        if(!Multiplayer.IsServer()) 
            return;
        OnStart();
        ShouldProcess = true;
    }

    public void StopBehavior()
    {
        if(!Multiplayer.IsServer()) 
            return;
        OnEnd();
        ShouldProcess = false;
    }

    private bool CheckBehaviorState()
    {
        return manager.GetState();        
    }
}