using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack;

namespace Mdmc.Code.Game.Data.Decorators;

[GlobalClass]
public partial class EffectData : Resource
{    
    [Export] public Effect.EffectType Type { get; private set; } = Effect.EffectType.Potency;
    [Export] public float EffectValue { get; private set; } = 0;

    public Effect GetEffect()
    {
        var effect = new Effect()
        {
            EffectValue = EffectValue,
            Type = Type
        };
        return effect;
    }
}