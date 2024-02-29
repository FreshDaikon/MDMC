using Godot;
using PlayFab.CloudScriptModels;

namespace Daikon.Game;


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
        var nextSlot = OriginSkill.AssignedSlot + GetOffset();
        GD.Print("Offset Slot: " + nextSlot);
        nextSlot = Mathf.Wrap(nextSlot, 0, 4);
        var nextSkill = OriginSkill.Player.Arsenal.GetSkill(OriginSkill.AssignedContainerSlot, nextSlot);
        GD.Print("Next Skill : " + nextSkill.AssignedSlot);
        GD.Print("Was it indeed the correct trigger? : " + (TrigggerSkill == nextSkill));
        return TrigggerSkill == nextSkill;
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