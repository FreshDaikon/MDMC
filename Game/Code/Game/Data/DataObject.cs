using Godot; 
using Daikon.Helpers;

namespace Daikon.Game;

[GlobalClass]
public partial class DataObject : Resource
{
    [ExportGroup("Data Object Properties")]
    [Export]
    private DataID ID;
    public int Id
    {
        get { return ID.Id; }
    }
    [Export]
    public PackedScene Scene;
    [ExportGroup("UI Properties")]
    [Export]
    public string Name = "Some Object.";
    [Export]
    public string Description = "Some Object Description.";
    [Export]
    public Texture2D Image;
    [Export]
    public Texture2D Icon;

}