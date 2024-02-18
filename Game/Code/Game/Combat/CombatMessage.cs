using Daikon.Helpers;

namespace Daikon.Game;

public class CombatMessage
{
    public int Caster;
    public int Target;
    public string Effect;
    public int Value;
    public MD.CombatMessageType MessageType;
}