using Godot;
using Daikon.System; 

namespace Daikon.Game;

public partial class JumpTest : Skill
{   
    
    public override SkillResult TriggerResult()
    {
        if(!Multiplayer.IsServer())
            return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.NOT_SERVER };

        var target = Player.CurrentTarget.Status;
        var dmgDone = target.InflictDamage(AdjustedPotency);
        var message = new CombatMessage()
        {
            Caster = int.Parse(Player.Name),
            Target = int.Parse(Player.CurrentTarget.Name),
            Value = dmgDone,
            MessageType = MD.CombatMessageType.DAMAGE,
            Effect = "Jump Heal ya!"
        };
        CombatManager.Instance.AddCombatMessage(message);
        Rpc(nameof(SkillRealization), message.Value, (int)message.MessageType);
        return new SkillResult(){ SUCCESS = true, result = MD.ActionResult.CAST};
    }
    public override SkillResult CheckSkill()
    {
        if(Player == null)
        {
            return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.ERROR };
        }
        var target = Player.CurrentTarget;
        if(target == null)
        { 
            MD.Log("Target is null");
            return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.INVALID_TARGET };
        }
        return new SkillResult() { SUCCESS = true, result = MD.ActionResult.CAST };
    }
   

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public override void SkillRealization(int value, int type)
    {
        DamageNumberRealization realization = (DamageNumberRealization)DataManager.Instance.GetRealizationObjectFromPath(RealizeSkillPath);
        realization.Value = value;
        realization.Type = (MD.CombatMessageType)type;
        realization.SpawnWithTarget(Player.CurrentTarget.Controller, Player.Controller.Position);
        base.SkillRealization(value, type);
    }

}