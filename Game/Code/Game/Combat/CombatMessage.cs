using Mdmc.Code.System;

namespace Mdmc.Code.Game.Combat;

public class CombatMessage
{
    public int Caster;
    public int Target;
    public string Effect;
    public int Value;
    public MD.CombatMessageType MessageType;
}