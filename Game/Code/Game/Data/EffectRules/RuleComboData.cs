using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack;
using Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack.Rules;

namespace Mdmc.Code.Game.Data.EffectRules;

[GlobalClass]
public partial class RuleComboData: EffectRuleData
{
    [Export] public RuleCombo.ComboDirection Direction { get; private set; }
    
    public override EffectRule GetRule()
    {
        var newCombo = new RuleCombo()
        {
            TriggerEffectData = EffectData,
            Direction = Direction,
        };
        return newCombo;
    }
}