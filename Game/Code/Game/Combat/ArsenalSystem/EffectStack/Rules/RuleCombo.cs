using Godot;

namespace Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack.Rules;


public class RuleCombo: EffectRule
{
    public enum ComboDirection
    {
        Clockwise,
        CounterClockwise,
        Across
    }
    
    public ComboDirection Direction { get; init; }
    
    public override void TryResolve()
    {
        SetWasResolved(true);
    }

    public override bool CheckCondition()
    {
        if (IsConditional && !PreviousOutcome)
            return false;
        
        var nextSlot = OriginSkill.AssignedSlot + GetOffset();
        nextSlot = Mathf.Wrap(nextSlot, 0, 4);
        var nextSkill = OriginSkill.Player.Arsenal.GetSkill(OriginSkill.AssignedContainerSlot, nextSlot);
        return TriggerSkill == nextSkill;
    }

    private int GetOffset()
    {
        return Direction switch
        {
            ComboDirection.Clockwise => 1,
            ComboDirection.CounterClockwise => -1,
            ComboDirection.Across => 2,
            _ => 0
        };
    }
}