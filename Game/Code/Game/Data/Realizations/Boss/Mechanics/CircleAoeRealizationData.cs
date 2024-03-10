using Godot;
using Mdmc.Code.Game.Realization.Realizations.Boss.Mechanics;

namespace Mdmc.Code.Game.Data.Realizations.Boss.Mechanics;

[GlobalClass]
public partial class CircleAoeRealizationData: RealizationData
{
    public override Realization.Realization GetRealization()
    {
        var instance = Scene.Instantiate<CircleAoeRealization>();
        return instance;
    }
}