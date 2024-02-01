


using Godot;

public partial class DamageNumberRealization : RealizationObject
{    
    public Color color;
    public int Value;
    public MD.CombatMessageType Type;

    public override void Spawn(Vector3 worldPos)
    {
        base.Spawn(worldPos);
        color = Type switch
        {
            MD.CombatMessageType.DAMAGE => new Color("#cfc148"),
            MD.CombatMessageType.HEAL => new Color("#7fbf3f"),
            MD.CombatMessageType.ENMITY => new Color("#4da6c9"),
            MD.CombatMessageType.EFFECT => new Color("#9f38c2"),
            _ => new Color(),
        };
    }
    public override void SpawnWithTarget(Node3D target, Vector3 startPosition)
    {
        base.SpawnWithTarget(target, startPosition);
        color = Type switch
        {
            MD.CombatMessageType.DAMAGE => new Color("#cfc148"),
            MD.CombatMessageType.HEAL => new Color("#7fbf3f"),
            MD.CombatMessageType.ENMITY => new Color("#4da6c9"),
            MD.CombatMessageType.EFFECT => new Color("#9f38c2"),
            _ => new Color(),
        };
    }

    public override void OnEndStart()
    {        
        PlayerHUD.Instance.SpawnDamageNumber(Value.ToString(), color, Position);
        base.OnEndStart();
    }
}