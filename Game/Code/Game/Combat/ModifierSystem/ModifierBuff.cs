using Godot;

namespace Mdmc.Code.Game.Combat.ModifierSystem;

[GlobalClass]
public abstract partial class ModifierBuff: Node
{
    [Export] protected ModifierHandler Modifier { get; private set; }
    //What could we actually add here... hmmm..
}