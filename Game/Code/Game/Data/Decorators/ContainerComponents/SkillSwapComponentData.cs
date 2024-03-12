using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem.ContainerComponents;

namespace Mdmc.Code.Game.Data.Decorators;

[GlobalClass]
public partial class SkillSwapComponentData : SkillContainerComponentData
{
    [Export] private int _swapSlot;
    [Export] private SkillData[] _swapSkills;
    [Export] private int _resourceGenerated;

    public override ContainerComponent GetComponent()
    {
        var newSwapper = new SkillSwapComponent()
        {  
            SwapSkillsData = _swapSkills,
            SwapSlot = _swapSlot,
            ResourceGenerated = _resourceGenerated,
        };
        return newSwapper;
    }
}