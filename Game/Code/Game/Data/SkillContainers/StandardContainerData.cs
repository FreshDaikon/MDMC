using System.Linq;
using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem;

namespace Mdmc.Code.Game.Data.SkillContainers;

[GlobalClass]
public partial class StandardContainerData : SkillContainerData
{
    public override SkillContainer GetSkillContainer()
    {
        var container = new SkillContainer
        {
            Data = this,
            Skills = Skills,
            Components = ComponentDatas,
            GeneratesResource = GeneratesResource,
            MaxResource = MaxResource,
            BuffsGranted = Modifiers.ToList()
        };
        return container;
    }
}