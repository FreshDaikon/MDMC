using Godot;

namespace Mdmc.Code.Game.Combat.SkillSystem;

public abstract partial class SkillAction: Node
{
    [ExportCategory("Optional")]
    [Export] protected PackedScene _realizationScene;

    [Signal] public delegate void ActionTriggeredEventHandler();
    
    public abstract void TriggerAction();
    public abstract bool CanTrigger();
}