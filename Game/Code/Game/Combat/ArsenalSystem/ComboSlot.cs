

using Godot;


[Tool]
[GlobalClass]
public partial class ComboSlot : Resource
{
    [Export]
    public int ComboSlotIndex { get; set; }
    [Export]
    public float ComboPotecyBonus { get; set; }
    [Export]
    public float FailurePotencyPenalty { get; set; }
}