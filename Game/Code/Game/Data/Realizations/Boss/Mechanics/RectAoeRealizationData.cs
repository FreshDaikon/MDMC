using Godot;

namespace Mdmc.Code.Game.Data.Realizations.Boss.Mechanics;

[GlobalClass]
public partial class RectAoeRealizationData: RealizationData
{
    public override Realization.Realization GetRealization()
    {
        var instance = Scene.Instantiate<Realization.Realizations.Boss.Mechanics.RectAoeRealization>();
        return instance;
    }
}