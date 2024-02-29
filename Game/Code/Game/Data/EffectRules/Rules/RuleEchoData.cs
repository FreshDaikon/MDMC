using Daikon.Helpers;
using Godot;

namespace Daikon.Game.EffectRules.Rules;

[GlobalClass]
public partial class RuleEchoData: EffectRuleData
{
    [Export]
    public MD.SkillType TypeToEcho { get; set; }
    
    public override EffectRule GetRule()
    {
        var newRule = new RuleEcho
        {
            TypeToEcho = TypeToEcho,
            Effect = effect
        };
        return newRule;
    }
}