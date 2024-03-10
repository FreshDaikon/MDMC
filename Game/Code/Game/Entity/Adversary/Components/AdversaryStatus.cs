using Godot;
using Mdmc.Code.Game.Entity.Components;

namespace Mdmc.Code.Game.Entity.Adversary.Components;

public partial class AdversaryStatus : EntityStatus
{
    [Signal]
    public delegate void ThreatInflictedEventHandler(int threat, Entity entity);

    public void InflictThreat(int threat, Entity entity) 
    {
        EmitSignal(SignalName.ThreatInflicted, threat, entity);
    }
}