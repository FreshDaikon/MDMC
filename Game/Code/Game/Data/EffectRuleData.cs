using Godot;

namespace Daikon.Game;

[GlobalClass]
public abstract partial class EffectRuleData: Resource
{
    [Export] public EffectData EffectData { get; set; }
    [Export] public bool IsConditional { get; set; }
    
    public abstract EffectRule GetRule();
}