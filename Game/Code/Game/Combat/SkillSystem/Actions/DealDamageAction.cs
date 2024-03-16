using System;
using Godot;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.RealizationSystem;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Combat.SkillSystem.Actions;

[GlobalClass]
public partial class DealDamageAction : SkillAction
{
    [Export] private SkillHandler _skill;
    [Export] private TargetAcquisition _acquisition;
    [Export] private int _potency;
    
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
        if(targets.Count > 0)
        {
            foreach(var entity in targets)
            {
                var result = entity.Status.InflictDamage(_potency, player);
                var message = new CombatMessage
                {
                    Caster = player.Id,
                    Target = entity.Id,
                    Value = result,
                    MessageType = MD.CombatMessageType.DAMAGE,
                    Effect = "X damaged Y"
                };
                CombatManager.Instance.AddCombatMessage(message);
                Rpc(nameof(RealizeAction), entity.Id, result);
            }
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
            RealizationManager.Instance.SpawnDamageNumber(value, entity.Controller.GlobalPosition, MD.GetSkillTypeColor(MD.SkillType.DPS));
        };      
    }

}