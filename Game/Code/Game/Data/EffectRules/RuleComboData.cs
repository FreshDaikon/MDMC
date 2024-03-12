using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack;
using Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack.Rules;

namespace Mdmc.Code.Game.Data.EffectRules;

[GlobalClass]
public partial class RuleComboData: EffectRuleData
{
    [Export] public RuleCombo.ComboDirection Direction { get; private set; }
    
    public override Rule GetRule()
    {
        var newCombo = new RuleCombo()
        {
            Charges = Charges,
            TriggerEffectData = EffectData,
            Direction = Direction,
        };
        return newCombo;
    }
}