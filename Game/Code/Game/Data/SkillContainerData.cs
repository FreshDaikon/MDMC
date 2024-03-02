using Godot;
using Daikon.Helpers;
using System.Linq;

namespace Daikon.Game;

[GlobalClass]
public partial class SkillContainerObject : DataObject
{
    [ExportGroup("Skill Container")] 
    [Export]
    public float BaseGcd { get; private set; } = 1.5f;
    [Export]
    public SkillSlotData[] skillSlots { get; private set; }
    [ExportGroup("Buffs Granted")]
    [Export]
    public Godot.Collections.Array<ModifierObject> Modifiers { get; private set; }
    
    public SkillContainer GetSkillContainer()
    {
        var instance = Scene.Instantiate<SkillContainer>();
        instance.BaseGcd = BaseGcd;
        instance.Data = this;
        instance.SkillSlots = skillSlots;
        instance.BuffsGranted = Modifiers.ToList();
        return instance;
    }
}