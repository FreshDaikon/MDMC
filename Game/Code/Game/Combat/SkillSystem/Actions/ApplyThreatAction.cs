using Godot;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.Entity.Adversary.Components;
using Mdmc.Code.Game.RealizationSystem;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Combat.SkillSystem.Actions;

[GlobalClass]
public partial class ApplyThreatAction : SkillAction
{
    [Export] private SkillHandler _skill;
    [ExportCategory("Acquisition should only apply adversaries!")]
    [Export] private TargetAcquisition _acquisition;
    [ExportCategory("Threat Value - Can be negative!")]
    [Export] private int _threat;

    public override bool CanTrigger()
    {
        if(_acquisition.CanAcquireTargets())
        {
            _acquisition.StoreTargets();
        }
        return _acquisition.CanAcquireTargets();
    }

    public override void TriggerAction()
    {
        var player = _skill.Arsenal.Player;
        var targets = _acquisition.StoredTargets;
        foreach(var entity in targets)
        {
            var adversaryStatus = entity.Status as AdversaryStatus;
            adversaryStatus.InflictThreat(_threat, player);
            Rpc(nameof(RealizeAction), entity.Id, _threat);
        }
    }
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RealizeAction(int target, int value)
    {
        if(_realizationScene == null ) return;
        
        var entity = ArenaManager.Instance.GetCurrentArena().GetEntity(target);
        
        if(entity == null) return;

        var builder = RealizationManager.Instance.CreateRealizationBuilder();
        var real = builder.New(_realizationScene, 3)
            .WithStartPosition(_skill.Arsenal.Player.Controller.GlobalPosition)
            .WithTarget(entity.Controller, 10)
            .Spawn();      
        real.OnRealizationEnd += () => {
            RealizationManager.Instance.SpawnDamageNumber(value, entity.Controller.GlobalPosition, MD.GetSkillTypeColor(MD.SkillType.TANK));
        };      
    }
}