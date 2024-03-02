using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

[GlobalClass]
public partial class ArenaObject : DataObject
{

    public Arena GetArena()
    {
        var instance = Scene.Instantiate<Arena>();
        instance.Data = this;
        return instance;
    }

}