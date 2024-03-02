using Daikon.Game.Realizations;
using Godot;

namespace Daikon.Game.Skills;

[GlobalClass]
public partial class SinglePotencyData: SkillData
{
    [Export] private ChaseTargetRealizationData _onSkillCastRealizationData;
    
    public override Skill GetSkill()
    {
        var skill = new SinglePotency();
        skill.Data = this;
        skill.IsUniversalSkill = IsUniversalSkill;
        skill.TimerType = TimerType;
        skill.ActionType = ActionType;
        skill.BasePotency = BasePotency;
        skill.Range = Range;
        skill.Cooldown = Cooldown;
        skill.CanMove = CanMove;
        skill.CastTime = CastTime;
        skill.ChannelTime = ChannelTime;
        skill.TickRate = TickRate;
        skill.ThreatMultiplier = ThreatMultiplier;
        // Realizations:
        skill.OnSkillCastRealization = _onSkillCastRealizationData;
        // Rules:
        skill.Rules = Rules;
        
        // Finally pass it back :
        return skill;
    }
}