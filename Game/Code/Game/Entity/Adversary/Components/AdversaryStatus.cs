using System;
using Godot;

namespace Daikon.Game;

public partial class AdversaryStatus : EntityStatus
{
    [Signal]
    public delegate void ThreatInflictedEventHandler(int threat, Entity entity);

    public void InflictThreat(int threat, Entity entity) 
    {
        EmitSignal(SignalName.ThreatInflicted, threat, entity);
    }
}