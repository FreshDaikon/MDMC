using Godot;
using Mdmc.Code.Game.Arena;

namespace Mdmc.Code.Game.Data;

[GlobalClass]
public partial class ArenaData : DataObject
{
    [ExportGroup("Scene to Load:")]
    [Export]
    public PackedScene Scene;

    public ArenaInstance GetArena()
    {
        var instance = Scene.Instantiate<ArenaInstance>();
        instance.Data = this;
        return instance;
    }

}