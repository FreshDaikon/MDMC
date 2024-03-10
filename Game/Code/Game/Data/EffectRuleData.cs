using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack;
using Mdmc.Code.Game.Data.Decorators;

namespace Mdmc.Code.Game.Data;

[GlobalClass]
public abstract partial class EffectRuleData: Resource
{
    [Export] public EffectData EffectData { get; set; }
    [Export] public bool IsConditional { get; set; }
    
    public abstract EffectRule GetRule();
}