using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem;
using Mdmc.Code.Game.Data.Decorators;

namespace Mdmc.Code.Game.Data;

[GlobalClass]
public abstract partial class SkillContainerData : DataObject
{
    [ExportGroup("Skill Container")] 
    [Export] public Color ContainerColor;
    [Export] public SkillData[] Skills {get; private set; }
    [ExportGroup("Buffs Granted")]
    [Export] public ModifierData[] Modifiers { get; private set; }
    [ExportGroup("Resource Generation")]
    [Export] public bool GeneratesResource { get; private set; }
    [Export] public int MaxResource { get; private set; }
    [ExportGroup("Component Data")]
    [Export] public SkillContainerComponentData[] ComponentDatas { get; private set; }

    public abstract SkillContainer GetSkillContainer();

}