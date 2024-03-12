

using Godot;

namespace Mdmc.Code.Game.Combat.ArsenalSystem.ContainerComponents;

public abstract partial class ContainerComponent: Node
{
    public SkillContainer Container;

    [Signal] public delegate void ComponentResolvedEventHandler();

    public abstract void SetupComponent();

    public abstract void UpdateComponent();

    public abstract void ResolveComponent();

}