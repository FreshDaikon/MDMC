using System.Collections.Generic;
using System.Linq;
using Godot;
using Mdmc.Code.Game.Data.Decorators;

namespace Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack;

public class RuleStack
{
    private Queue<Rule> _currentRules = new();
    private List<Rule> _storedRules = new();
    
    public RuleStack(){}
    
    public Skill LastSkill { get; set; }
    public bool PreviousRuleOutcome = false;

    public List<Effect> EffectStack { get; private set; } = new();

    public void AddRule(Rule effectRule)
    {
        _currentRules.Enqueue(effectRule);
    }
    
    public void ResolveRules(Skill instigator)
    {
        _storedRules.ForEach(e => _currentRules.Enqueue(e));
        _storedRules.Clear();
        
        EffectStack.Clear();

        while (_currentRules.Count > 0)
        {
            var currentRule = _currentRules.Dequeue();
            currentRule.ArmTrigger(instigator);
            var result = currentRule.Trigger();
            if(!result)
            {
                PreviousRuleOutcome = false;
                if (currentRule.WasResolved) continue;
                // else:
                _storedRules.Add(currentRule);
            }
            else
            {
                PreviousRuleOutcome = true;
                EffectStack.Add(currentRule.GetEffect());   
                if(!currentRule.WasResolved) _storedRules.Add(currentRule);
            }
        }
    }
}