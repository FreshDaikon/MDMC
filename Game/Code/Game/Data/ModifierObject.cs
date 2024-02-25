using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

[GlobalClass]
public partial class ModifierObject : DataObject
{

    public Modifier GetModifier()
    {
        var instance = Scene.Instantiate<Modifier>();
        instance.Data = this;
        return instance;
    }

}