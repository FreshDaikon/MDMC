using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem;
using Mdmc.Code.Game.Combat.ArsenalSystem.Skills.BasicTemplates;
using Mdmc.Code.Game.Data.Realizations;

namespace Mdmc.Code.Game.Data.Skills;

[GlobalClass]
public partial class SinglePotencyData: SkillData
{
    [Export] private ChaseTargetRealizationData _onSkillCastRealizationData;
    
    public override Skill GetSkill()
    {
        var skill = new SinglePotency
        {
            Data = this,
            IsUniversalSkill = IsUniversalSkill,
            TimerType = TimerType,
            ActionType = ActionType,
            BasePotency = BasePotency,
            RequiresResource = RequiresResource,
            RequiredAmountOfResource = RequiredAmountOfResource,
            Range = Range,
            Cooldown = Cooldown,
            CanMove = CanMove,
            CastTime = CastTime,
            ChannelTime = ChannelTime,
            TickRate = TickRate,
            ThreatMultiplier = ThreatMultiplier,
            // Realizations:
            OnSkillCastRealization = _onSkillCastRealizationData,
            // Rules:
            Rules = Rules
        };
        // Finally pass it back :
        return skill;
    }
}