

using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem;
using Mdmc.Code.System;
using static Mdmc.Code.Game.Combat.Modifiers.SkillTriggers.ModSkillTrigger;

public partial class ModSkillTriggerData: Resource
{    
    [Export] public TriggerType Type;
    [Export] public TriggerEffect Effect;
    [Export] public float EffectValue;
    [Export] public MD.SkillType TypeToCheck;
}