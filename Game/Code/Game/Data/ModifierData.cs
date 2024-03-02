using Godot;
using Daikon.Helpers;
using static Daikon.Game.Modifier;
using System.Linq;

namespace Daikon.Game;

[GlobalClass]
public partial class ModifierObject : DataObject
{
    [ExportGroup("Modifier Settings")]
    [Export]
    public bool IsPermanent = false;
    [Export]
    public float Duration = 5f;
    [Export]
    public bool IsTicked = false;
    [Export(PropertyHint.Range, "1, 3")]
    public float TickRate = 1f;
    [Export]
    public bool CanStack = false;
    [Export]
    public int Stacks = 1;
    [Export]
    public int MaxStacks = 1;
    [Export]
    private Godot.Collections.Array<ModTags> Tags { get; set; }
    [Export]
    public double ModifierValue = 0; // This is very specific per mod!

    public Modifier GetModifier()
    {
        var instance = Scene.Instantiate<Modifier>();
        instance.IsPermanent = IsPermanent;
        instance.Duration = Duration;
        instance.IsTicked = IsTicked;
        instance.TickRate = TickRate;
        instance.CanStack = CanStack;
        instance.MaxStacks = MaxStacks;
        instance.Tags = Tags.ToList();
        instance.ModifierValue = ModifierValue; // This is very specific per mod!
        instance.RemainingValue = ModifierValue; // Only used for exhaustible mods!
        instance.Data = this;
        return instance;
    }

}