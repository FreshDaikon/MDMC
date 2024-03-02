using Daikon.Game.EffectRules;
using Godot;
using Daikon.Helpers;
using Godot.Collections;

namespace Daikon.Game;

[GlobalClass]
public abstract partial class SkillData : DataObject
{
    [ExportGroup("Skill Options:")]
    [Export] public bool IsUniversalSkill = false;
    [Export] public MD.SkillTimerType TimerType;
    [Export] public int BasePotency = 100;
    [Export] public float Range = 10f;
    [Export] public float Cooldown = 1f;
    [ExportCategory("Action Type")]
    [Export] public MD.SkillActionType ActionType;
    [ExportCategory("Casting Skill Options")]
    [Export] public bool CanMove = false;
    [Export] public float CastTime = 0f;
    [ExportCategory("Channeling SKill Options")]
    [Export] public float ChannelTime = 0f;
    [Export] public float TickRate = 1f;
    [Export] public float ThreatMultiplier = 1f;
    // Effects and rules:
    [ExportCategory("Skill Effects :")]
    [Export] public EffectRuleData[] Rules { get; private set; }
    // Realizations:
    
    // Methods that needs overwriting :
    public abstract Skill GetSkill();

}