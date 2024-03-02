using System.Linq;
using Godot;

namespace Daikon.Game.SkillContainers;

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