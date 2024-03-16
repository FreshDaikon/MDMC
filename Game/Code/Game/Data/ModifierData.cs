using Godot;
using Mdmc.Code.Game.Combat.ModifierSystem;
using static Mdmc.Code.Game.Combat.ModifierSystem.ModifierHandler;

namespace Mdmc.Code.Game.Data;

[GlobalClass]
public partial class ModifierData : DataObject
{
    [Export] private PackedScene ModifierScene; 

    public ModifierHandler GetModifier()
    {
        var mod = ModifierScene.Instantiate() as ModifierHandler;
        mod.Data = this;
        return mod;
    }
}