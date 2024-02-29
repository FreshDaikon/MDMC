using Godot;

namespace Daikon.Game.EffectRules;

[GlobalClass]
public abstract partial class EffectRuleData: Resource
{
    [Export] public Effect effect { get; set; }
    [Export] public bool isConditional { get; set; }
    public abstract EffectRule GetRule();
}