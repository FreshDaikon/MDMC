using System.Collections.Generic;
using Godot;

namespace Daikon.Game;

public class EffectStack
{
    private Queue<EffectRule> _currentRules = new();
    private List<EffectRule> _storedRules = new();
    
    private PlayerArsenal _arsenal;
    
    public EffectStack(PlayerArsenal owner) => _arsenal = owner;
    
    public Skill LastSkill { get; set; }
    public bool PreviousRuleOutcome = false;
    public List<EffectData> Stack { get; private set; } = new();

    public void AddRule(EffectRule effectRule)
    {
        _currentRules.Enqueue(effectRule);
    }
    
    public List<EffectData> ResolveEffects(Skill instigator)
    {
        GD.Print("Resolving any effects..");
        Stack.Clear();
        _storedRules.ForEach(e => _currentRules.Enqueue(e));
        _storedRules.Clear();
        GD.Print("The number of rules to check is :" + _currentRules.Count);
        while (_currentRules.Count > 0)
        {
            var currentRule = _currentRules.Dequeue();
            currentRule.SetTrigger(instigator, PreviousRuleOutcome);
            var effect = currentRule.Trigger();
            GD.Print("Current effect result:" + effect.Type);
            if(effect.Type == EffectData.EffectType.Failed)
            {
                PreviousRuleOutcome = false;
                GD.Print("Effect was failed!");
                if (currentRule.WasResolved)
                {
                    GD.Print("Effect was failed and resolved");
                    continue;
                }
                _storedRules.Add(currentRule);
            }
            else
            {
                PreviousRuleOutcome = true;
                GD.Print("Effect was added to the stack!");
                if(effect.Type != EffectData.EffectType.NoEffect) Stack.Add(effect);   
                if(!currentRule.WasResolved) _storedRules.Add(currentRule);
            }
        }
        return Stack;
    }
}