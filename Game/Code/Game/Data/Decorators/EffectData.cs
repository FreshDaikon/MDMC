using Godot;

namespace Daikon.Game;

[GlobalClass]
public partial class EffectData : Resource
{
    public enum EffectType
    {
        Failed,
        NoEffect,
        Trigger,
        Potency,
        Range,
        CastTime,
        Cooldown,
    }
    
    [Export]
    public EffectType Type { get; private set; } = EffectType.Failed;
    [Export]
    public double Value { get; private set; } = 0;
}