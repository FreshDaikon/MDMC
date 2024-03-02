using Godot;
using Daikon.Helpers;
using System.Linq;

namespace Daikon.Game;

[GlobalClass]
public abstract partial class SkillContainerData : DataObject
{
    [ExportGroup("Skill Container")] 
    [Export]
    public float BaseGcd { get; private set; } = 1.5f;
    [Export]
    public SkillSlotData[] skillSlots { get; private set; }
    [ExportGroup("Buffs Granted")]
    [Export]
    public Godot.Collections.Array<ModifierData> Modifiers { get; private set; }

    public abstract SkillContainer GetSkillContainer();
}