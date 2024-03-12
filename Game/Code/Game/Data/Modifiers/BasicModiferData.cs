using System.Linq;
using Godot;
using Mdmc.Code.Game.Combat.Modifiers;

namespace Mdmc.Code.Game.Data.Modifiers;

[GlobalClass]
public partial class BasicModiferData: ModifierData
{
    public override Modifier GetModifier()
    {
        var modifier = new Modifier
        {
            Type = Type,
            IsPermanent = IsPermanent,
            Duration = Duration,
            IsTicked = IsTicked,
            TickRate = TickRate,
            CanStack = CanStack,
            MaxStacks = MaxStacks,
            Tags = Tags.ToList(),
            ModifierValue = ModifierValue, // This is very specific per mod!
            RemainingValue = ModifierValue, // Only used for exhaustible mods! (Like shields for example..)
            Charges = Charges,
            SkillTriggerData = ModTriggerData,
            Data = this
        };
        return modifier;
    }
}