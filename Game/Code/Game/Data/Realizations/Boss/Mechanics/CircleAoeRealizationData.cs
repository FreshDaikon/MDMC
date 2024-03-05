using Godot;

namespace Daikon.Game.Realizations.Boss.Mechanics;

[GlobalClass]
public partial class CircleAoeRealizationData: RealizationData
{
    public override Realization GetRealization()
    {
        var instance = Scene.Instantiate<CircleAoeRealization>();
        return instance;
    }
}