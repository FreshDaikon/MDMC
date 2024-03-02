using Daikon.Game.EffectRules;
using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

[GlobalClass]
public partial class SkillObject : DataObject
{
    [ExportGroup("Skill Options:")]
    [Export]
    public bool IsUniversalSkill = false;
    [Export]
    public MD.SkillTimerType TimerType;
    [Export]
    public int BasePotency = 100;
    [Export]
    public float Range = 10f;
    [Export]
    public float Cooldown = 1f;
    [ExportCategory("Action Type")]
    [Export]
    public MD.SkillActionType ActionType;
    [ExportCategory("Casting Skill")]
    [Export]
    public bool CanMove = false;
    [Export]
    public float CastTime = 0f;
    [ExportCategory("Channeling SKill")]
    [Export]
    public float ChannelTime = 0f;
    [Export]
    public float TickRate = 1f;
    [Export]
    public float ThreatMultiplier = 1f;
    
    [Export]
    public EffectRuleData[] Rules { get; private set; }

    [ExportGroup("Skill Realizations")]
    [Export]
    public RealizationObject RealizeOnCast;
    [Export]
    public RealizationObject RealizeOnFinish;    
    [Export]
    public RealizationObject RealizeOnSkill;

    public Skill GetSkill()
    {
        var skill = new Skill();
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