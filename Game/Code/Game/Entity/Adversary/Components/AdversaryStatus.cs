using System;
using Godot;

namespace Daikon.Game;

public partial class AdversaryStatus : EntityStatus
{
    [Signal]
    public delegate void ThreatInflictedEventHandler(float threat, Entity entity);

    public void InflictThreat(float threat, Entity entity) 
    {
        EmitSignal(SignalName.ThreatInflicted, threat, entity);
    }
}