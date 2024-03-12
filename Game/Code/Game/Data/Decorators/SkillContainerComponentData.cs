using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem.ContainerComponents;

namespace Mdmc.Code.Game.Data.Decorators;

[GlobalClass]
public abstract partial class SkillContainerComponentData : Resource
{
    public abstract ContainerComponent GetComponent();
}