using Godot;

namespace Daikon.Game;

[GlobalClass]
public partial class AutoAttack: BaseBehavior
{   
    [Export]
    private int attackDamage;
    [Export]
    private double attackTime = 5;
    private double lastAttack;

    public override void ProcessBehavior()
    {
        var topThreat = Manager.Entity.GetThreatEntity(0);
        if(topThreat == null)
        {
            StopBehavior();
            return;
        }
        if(GameManager.Instance.GameClock - lastAttack > attackTime)
        {
            //auto attack
            Attack(topThreat);
            lastAttack = GameManager.Instance.GameClock;
        }
        base.ProcessBehavior();
    }

    public override void OnStart()
    {
        lastAttack = GameManager.Instance.GameClock;
        base.OnStart();
    }

    private void Attack(Entity entity)
    {
        var dmg = entity.Status.InflictDamage(attackDamage, Manager.Entity);
        Rpc(nameof(SpawnDamageNumber), entity.Controller.GlobalPosition, dmg);
    }
    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SpawnDamageNumber(Vector3 position, int value)
    {
        RealizationManager.Instance.SpawnDamageNumber(value, position, new Color("#ff0000"));
    }
}
 