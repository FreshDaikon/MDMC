using Godot;
using Daikon.Helpers;
using System.Linq;

namespace Daikon.Game;

[GlobalClass]
public partial class SkillContainerObject : DataObject
{
    [ExportGroup("Skill Container")]
    [Export]
    public SkillSlotData[] skillSlots;
    [ExportGroup("Buffs Granted")]
    [Export]
    public Godot.Collections.Array<ModifierObject> Modifiers { get; set; }
    
    public SkillContainer GetSkillContainer()
    {
        var instance = Scene.Instantiate<SkillContainer>();
        instance.Data = this;
        instance.SkillSlots = skillSlots;
        instance.BuffsGranted = Modifiers.ToList();
        return instance;
    }
}