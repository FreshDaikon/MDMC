using Godot;
using Mdmc.Code.Game.Realization.Realizations.General;

namespace Mdmc.Code.Game.Data.Realizations;

[GlobalClass]
public partial class ChaseTargetRealizationData: RealizationData
{
    public override Realization.Realization GetRealization()
    {
        var instance = Scene.Instantiate<ChaseTargetRealization>();
        return instance;
    }
}