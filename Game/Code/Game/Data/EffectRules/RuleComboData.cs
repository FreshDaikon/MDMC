using Godot;

namespace Daikon.Game.EffectRules.Rules;

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