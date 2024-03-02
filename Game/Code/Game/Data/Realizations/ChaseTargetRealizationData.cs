using Daikon.Game.General;
using Godot;

namespace Daikon.Game.Realizations;

[GlobalClass]
public partial class ChaseTargetRealizationData: RealizationData
{
    public override Realization GetRealization()
    {
        var instance = Scene.Instantiate<ChaseTargetRealization>();
        return instance;
    }
}