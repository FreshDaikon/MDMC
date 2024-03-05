using Godot;

namespace Daikon.Game.Realizations.Boss.Mechanics;

[GlobalClass]
public partial class RectAoeRealizationData: RealizationData
{
    public override Realization GetRealization()
    {
        var instance = Scene.Instantiate<RectAoeRealization>();
        return instance;
    }
}