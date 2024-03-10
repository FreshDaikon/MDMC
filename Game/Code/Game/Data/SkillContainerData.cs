using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem;

namespace Mdmc.Code.Game.Data;

[GlobalClass]
public abstract partial class SkillContainerData : DataObject
{
    [ExportGroup("Skill Container")] 
    [Export] public Color ContainerColor;
    [Export] public float BaseGcd { get; private set; } = 1.5f;
    [Export] public Decorators.SkillSlotData[] skillSlots { get; private set; }
    [ExportGroup("Buffs Granted")]
    [Export] public Godot.Collections.Array<ModifierData> Modifiers { get; private set; }

    public abstract SkillContainer GetSkillContainer();
}