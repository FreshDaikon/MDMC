using Godot;
using Mdmc.Code.Game.Data.Decorators;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack;


public class Effect
{
    public enum EffectType
    {
        Cooldown,
        Potency,
        CastTime,
    }
    // Set:
    public EffectType Type { get; init; }
    public float EffectValue { get; init; }
}