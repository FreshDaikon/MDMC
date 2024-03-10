using Godot;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Data;

[GlobalClass]
public partial class DataObject : Resource
{
    [ExportGroup("Data Object Properties")]
    [Export]
    private DataID ID;
    public int Id => ID.Id;

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