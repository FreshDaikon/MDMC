using Godot;
using Mdmc.Code.Game.RealizationSystem;

namespace Mdmc.Code.Game.Entity.Adversary.Actions.Behaviors;

[GlobalClass]
public partial class AutoAttack: BaseBehavior
{   
    [Export] private int _attackDamage;
    [Export] private double _attackTime = 5;
    
    private double lastAttack;

    public override void ProcessBehavior()
    {
        var topThreat = Manager.Entity.GetThreatEntity(0);
        if(topThreat == null)
        {
            StopBehavior();
            return;
        }
        if(GameManager.Instance.GameClock - lastAttack > _attackTime)
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
        var dmg = entity.Status.InflictDamage(_attackDamage, Manager.Entity);
        Rpc(nameof(SpawnDamageNumber), entity.Controller.GlobalPosition, dmg);
    }
    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SpawnDamageNumber(Vector3 position, int value)
    {
        RealizationManager.Instance.SpawnDamageNumber(value, position, new Color("#ff0000"));
    }
}
 