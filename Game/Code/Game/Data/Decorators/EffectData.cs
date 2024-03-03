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
    
    public enum DataType
    {
        FreeCast
    }
    
    [Export]
    public EffectType Type { get; private set; } = EffectType.Failed;
    [Export]
    public double Value { get; private set; } = 0;
    
    public dynamic ExtraData { get; set; } 
}