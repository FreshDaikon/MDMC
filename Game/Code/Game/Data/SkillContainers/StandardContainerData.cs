using System.Linq;
using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem;

namespace Mdmc.Code.Game.Data.SkillContainers;

[GlobalClass]
public partial class StandardContainerData : SkillContainerData
{
    public override SkillContainer GetSkillContainer()
    {
        var container = new SkillContainer();
        container.BaseGcd = BaseGcd;
        container.Data = this;
        container.SkillSlots = skillSlots;
        container.BuffsGranted = Modifiers.ToList();
        return container;
    }
}