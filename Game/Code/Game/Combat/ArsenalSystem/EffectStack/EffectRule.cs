using Godot;

namespace Daikon.Game.EffectStack;

public partial class EffectRule: Resource
{
    
    private Effect _effect;
    private bool _wasResolved = true;
    
    public Effect Trigger(Skill instigator, PlayerArsenal owner)
    {
        return CheckCondition() ? _effect : new Effect(Effect.EffectType.NoEffect, 0);
    }

    protected virtual void SetWasResolved() { }

    protected virtual bool CheckCondition()
    {
        return false; 
    }

    public virtual bool WasResolved()
    {
        return _wasResolved;
    }
}