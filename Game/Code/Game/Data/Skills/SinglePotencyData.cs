using Godot;

namespace Daikon.Game.Skills;

[GlobalClass]
public partial class BasicPotencyData: SkillData
{
    public override Skill GetSkill()
    {
        var skill = new SinglePotency();
        //Implement Setting up stuff:
        skill.Data = this;
        skill.IsUniversalSkill = IsUniversalSkill;
        skill.TimerType = TimerType;
        skill.BasePotency = BasePotency;
        skill.Range = Range;
        skill.Cooldown = Cooldown;
        skill.CanMove = CanMove;
        skill.CastTime = CastTime;
        skill.ChannelTime = ChannelTime;
        skill.TickRate = TickRate;
        skill.ThreatMultiplier = ThreatMultiplier;
        // Setup Realizations :
        skill.RealizeOnCast = RealizeOnCast;
        skill.RealizeOnFinish = RealizeOnFinish;
        skill.RealizeOnSkill = RealizeOnSkill;

        skill.Rules = Rules;
        
        // Finally pass it back :
        return skill;
    }
}