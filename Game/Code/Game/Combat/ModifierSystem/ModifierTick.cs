using Godot;

namespace Mdmc.Code.Game.Combat.ModifierSystem;

[GlobalClass]
public abstract partial class ModifierTick : Node
{
    [Export] protected ModifierHandler _modifier { get ; private set; }

    [ExportCategory("Optional")]
    [Export] protected PackedScene _realizationScene;
    [Signal] public delegate void TickedEventHandler();
    
    public abstract void Tick();
    public abstract bool CanTick();
}