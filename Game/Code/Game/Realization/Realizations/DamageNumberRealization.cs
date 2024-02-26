using Godot;
using Daikon.Helpers;
using Daikon.Client;

namespace Daikon.Game;

public partial class DamageNumberRealization : Realization
{    
    public Color color;
    public int Value;

    public override void ResolveExtraData()
    {
        if(ExtraData == null)
        {
            GD.PushError("Damage number realizatio expects extra data! in the form { Color : '#somecolor', Value: 123 }");
            Kill();
        }
        else
        {
            int type = ExtraData.Type; 
            color = (MD.CombatMessageType)type switch
            {
                MD.CombatMessageType.DAMAGE => new Color("#cfc148"),
                MD.CombatMessageType.HEAL => new Color("#7fbf3f"),
                MD.CombatMessageType.ENMITY => new Color("#4da6c9"),
                MD.CombatMessageType.EFFECT => new Color("#9f38c2"),
                _ => new Color(),
            };
            Value = ExtraData.Value;            
        }
    }

    public override void OnEndStart()
    {        
        UIHUDMain.Instance.SpawnDamageNumber(Value.ToString(), color, Position);
    }
}