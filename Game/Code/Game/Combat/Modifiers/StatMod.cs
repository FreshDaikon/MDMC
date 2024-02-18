using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

[GlobalClass]
public partial class StatMod : Resource
{
    [Export]
    public MD.ModCategory Category { get; set; }
    [Export]
    public float Value { get; set; }
    [Export]
    public string Name { get; set; }
    [Export]
    public Texture2D Icon { get; set; }    

    public StatMod()
    {
        Category = MD.ModCategory.SPEED;
        Name = "Default";
        Icon = null;
        Value = 0f;
    }
}