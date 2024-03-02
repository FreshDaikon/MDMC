using System.Linq;
using Godot;

namespace Daikon.Game.Modifiers;

[GlobalClass]
public partial class BasicModiferData: ModifierData
{
    public override Modifier GetModifier()
    {
        var modifier = new Modifier();
        modifier.IsPermanent = IsPermanent;
        modifier.Duration = Duration;
        modifier.IsTicked = IsTicked;
        modifier.TickRate = TickRate;
        modifier.CanStack = CanStack;
        modifier.MaxStacks = MaxStacks;
        modifier.Tags = Tags.ToList();
        modifier.ModifierValue = ModifierValue; // This is very specific per mod!
        modifier.RemainingValue = ModifierValue; // Only used for exhaustible mods!
        modifier.Data = this;
        return modifier;
    }
}