using Godot;
using Mdmc.Code.Game.Combat.Modifiers;
using static Mdmc.Code.Game.Combat.Modifiers.Modifier;

namespace Mdmc.Code.Game.Data;

[GlobalClass]
public abstract partial class ModifierData : DataObject
{
    [ExportGroup("Modifier Settings")]
    [Export] public ModType Type;
    [Export] public bool IsPermanent = false;
    [ExportCategory("REMEMBER : only use saved mods - don't create inline!")]
    [Export]
    public float Duration = 5f;
    [Export]
    public bool IsTicked = false;
    [Export(PropertyHint.Range, "1, 3")]
    public float TickRate = 1f;
    [Export]
    public bool CanStack = false;
    [Export]
    public int Stacks = 1;
    [Export]
    public int MaxStacks = 1;
    [Export]
    public Godot.Collections.Array<ModTags> Tags { get; set; }
    [Export] public double ModifierValue = 0; // This is very specific per mod!
    [Export] public int Charges = 0;
    [Export] public ModSkillTriggerData ModTriggerData;

    public abstract Modifier GetModifier();

}