using System.Linq;
using Godot;
using Mdmc.Code.Game.Combat.Modifiers;

namespace Mdmc.Code.Game.Data.Modifiers;

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
        modifier.RemainingValue = ModifierValue; // Only used for exhaustible mods! (Like shields for example..)
        modifier.Data = this;
        return modifier;
    }
}