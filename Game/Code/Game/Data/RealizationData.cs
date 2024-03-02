using Daikon.Game;
using Godot;

[GlobalClass]
public partial class RealizationObject: DataObject
{
    [ExportGroup("Scene to Load:")]
    [Export]
    public PackedScene Scene; 
    
    // Note - this is a one shot Object!
    // Meaning it requires a scene!
    // NOT abstract - don't reimplement (yet!)
    public Realization GetRealization()
    {
         var instance = Scene.Instantiate<Realization>();
         return instance;
    }
}