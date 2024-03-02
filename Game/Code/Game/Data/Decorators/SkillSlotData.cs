using Daikon.Helpers;
using Godot;

[GlobalClass]
public partial class SkillSlotData : Resource
{
    [ExportGroup("Skill Slot Properties")]
    [Export]
    public MD.SkillType SlotSkillType { get; set; }
    [Export]
    public float PotencyMultiplier = 1.0f;
    [Export]
    public float ThreatMultiplier = 1.0f;
}