using Daikon.Game;
using Godot;

[GlobalClass]
public partial class RealizationObject: DataObject
{
    public Realization GetRealization()
    {
         var instance = Scene.Instantiate<Realization>();
         return instance;
    }
}