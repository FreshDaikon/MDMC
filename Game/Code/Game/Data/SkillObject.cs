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

    [ExportGroup("Skill Realizations")]
    [Export]
    public RealizationObject RealizeOnCast;
    [Export]
    public RealizationObject RealizeOnFinish;    
    [Export]
    public RealizationObject RealizeOnSkill;

    public Skill GetSkill()
    {
        var instance = Scene.Instantiate<Skill>();
        //Implement Setting up stuff:
        instance.Data = this;
        instance.IsUniversalSkill = IsUniversalSkill;
        instance.TimerType = TimerType;
        instance.BasePotency = BasePotency;
        instance.Range = Range;
        instance.Cooldown = Cooldown;
        instance.CanMove = CanMove;
        instance.CastTime = CastTime;
        instance.ChannelTime = ChannelTime;
        instance.TickRate = TickRate;
        instance.ThreatMultiplier = ThreatMultiplier;
        // Setup Realizations :
        instance.RealizeOnCast = RealizeOnCast;
        instance.RealizeOnFinish = RealizeOnFinish;
        instance.RealizeOnSkill = RealizeOnSkill;
        // Finally pass it back :
        return instance;
    }

}