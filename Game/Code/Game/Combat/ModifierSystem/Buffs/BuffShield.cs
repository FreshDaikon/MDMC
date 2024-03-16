using System;
using Godot;

namespace Mdmc.Code.Game.Combat.ModifierSystem.Buffs;

[GlobalClass]
public partial class BuffShield : ModifierBuff
{
    [Export] public int ShieldValue { get; private set; }

    public int ImpactShield(int value)
    {
        ShieldValue -= value;
        if(ShieldValue > 0) return 0;
        else
        {            
            Modifier.Terminate();
            return Math.Abs(ShieldValue);
        }
    }
}