
using Godot;

namespace Mdmc.Code.Game.Entity.Adversary.Actions.Behaviors;

[GlobalClass]
public partial class Chase: BaseBehavior
{   
    [Export]
    private float chaseDistance = 2f;
    [Export]
    private float chaseSpeed = 3f;


    public override void ProcessBehavior()
    {
        var topThreat = Manager.Entity.GetThreatEntity(0);
        if(topThreat == null)
        {
            StopBehavior();
            return;
        }
        
        var direction = topThreat.Controller.GlobalPosition - Manager.Entity.Controller.GlobalPosition;
        float distance = direction.Length();
        if(distance > chaseDistance)
        {
            Manager.Entity.Mover.SetDirection(direction.Normalized());
        }
        base.ProcessBehavior();
    }
}
 