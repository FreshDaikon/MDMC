using Godot;

namespace Daikon.Game;

[GlobalClass]
public partial class ArenaData : DataObject
{
    [ExportGroup("Scene to Load:")]
    [Export]
    public PackedScene Scene;

    // Note - this is a one shot Object!
    // Meaning it requires a scene!
    // NOT abstract - don't reimplement (yet!)
    public Arena GetArena()
    {
        var instance = Scene.Instantiate<Arena>();
        instance.Data = this;
        return instance;
    }

}