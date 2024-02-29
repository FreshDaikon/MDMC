namespace Daikon.Game.EffectStack;

public class Effect
{
    public Effect(EffectType type, double value)
    {
        Type = type;
        Value = value;
    }
    
    public enum EffectType
    {
        NoEffect,
        Trigger,
        Potency,
        Range,
        CastTime,
        Cooldown,
        
    }

    public EffectType Type { get; private set; } = EffectType.Trigger;
    public double Value { get; private set; } = 0;
}