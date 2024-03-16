using Godot;
using Mdmc.Code.Game.RealizationSystem;

namespace Mdmc.Code.Game.Combat.ModifierSystem.Ticks;

[GlobalClass]
public partial class HealAffectedTick : ModifierTick
{
    [Export] private int HealValue;

    public override bool CanTick()
    {
        return _modifier.Affected != null && _modifier.Terminated == false;
    }

    public override void Tick()
    {        
        _modifier.Affected.Status.InflictHeal(HealValue, _modifier.Affected);
        Rpc(nameof(RealizeTick));
    }


    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RealizeTick()
    {
        if(_realizationScene == null ) return;
        
        var entity = _modifier.Affected;
        var builder = RealizationManager.Instance.CreateRealizationBuilder();

        var real = builder.New(_realizationScene, 3)
            .InTransform(entity.Controller)
            .WithOffset(new Vector3(0f, entity.EntityHeight/2, 0f))
            .Spawn();      
    } 
}