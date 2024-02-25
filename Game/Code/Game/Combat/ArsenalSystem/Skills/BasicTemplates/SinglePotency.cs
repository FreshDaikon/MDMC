using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class SinglePotency : Skill
{
    public override SkillResult TriggerResult()
    {
        if(!Multiplayer.IsServer())
            return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.NOT_SERVER };

        if(IsUniversalSkill)
        {
            GD.PrintErr("This Skill can never be universal!");
            return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.INVALID_SETUP };
        }
        switch(SkillType)
        {
            case MD.SkillType.DPS:
            {
                var target = (AdversaryStatus)Player.CurrentTarget.Status;
                GD.Print("Trying to deal damage = " + AdjustedPotency);
                var dmgDone = target.InflictDamage(AdjustedPotency, Player);
                target.InflictThreat(dmgDone * ThreatMultiplier, Player);
                var message = new CombatMessage()
                {
                    Caster = int.Parse(Player.Name),
                    Target = int.Parse(Player.CurrentTarget.Name),
                    Value = dmgDone,
                    MessageType = MD.CombatMessageType.DAMAGE,
                    Effect = "X damaged Y"
                };
                CombatManager.Instance.AddCombatMessage(message);
                Rpc(nameof(SkillRealization), message.Value, (int)message.MessageType);
                return new SkillResult(){ SUCCESS = true, result = MD.ActionResult.CAST};
            };
            case MD.SkillType.HEAL:
            {
                var target = Player.CurrentFriendlyTarget.Status;
                var healDone = target.InflictHeal(AdjustedPotency, Player);
                var message = new CombatMessage()
                {
                    Caster = int.Parse(Player.Name),
                    Target = int.Parse(Player.CurrentFriendlyTarget.Name),
                    Value = healDone,
                    MessageType = MD.CombatMessageType.HEAL,
                    Effect = "X healed Y"
                };
                CombatManager.Instance.AddCombatMessage(message);
                Rpc(nameof(SkillRealization), message.Value, (int)message.MessageType);
                return new SkillResult(){ SUCCESS = true, result = MD.ActionResult.CAST};
            }                
            case MD.SkillType.TANK:
            {
                var target = (AdversaryStatus)Player.CurrentTarget.Status;
                var dmgDone = target.InflictDamage(AdjustedPotency, Player);
                target.InflictThreat(dmgDone * ThreatMultiplier, Player);
                var message = new CombatMessage()
                {
                    Caster = int.Parse(Player.Name),
                    Target = int.Parse(Player.CurrentTarget.Name),
                    Value = dmgDone,
                    MessageType = MD.CombatMessageType.DAMAGE,
                    Effect = "X damaged Y"
                };
                CombatManager.Instance.AddCombatMessage(message);
                Rpc(nameof(SkillRealization), message.Value, (int)message.MessageType);
                return new SkillResult(){ SUCCESS = true, result = MD.ActionResult.CAST};
            }
            default:
            {
                return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.INVALID_SETUP};
            }
        }        
    }

    public override SkillResult CheckSkill()
    {
        if(Player == null)
        {
            return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.ERROR };
        }
        if(IsUniversalSkill)
        {
            GD.PrintErr("This Skill can never be universal!");
            return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.INVALID_SETUP };
        }
        switch(SkillType)
        {
            case MD.SkillType.DPS:
            {
                var target = Player.CurrentTarget;
                if(target == null)
                    return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.INVALID_TARGET};
                var status = (AdversaryStatus)target.Status;
                if(status == null)
                {
                    return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.INVALID_TARGET};
                }
                else
                {
                    return new SkillResult(){ SUCCESS = true, result = MD.ActionResult.CAST};
                }
            };
            case MD.SkillType.HEAL:
            {
                var target = Player.CurrentTarget;
                if(target == null)
                    return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.INVALID_TARGET};
                var status = target.Status;
                if(status == null)
                {
                    return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.INVALID_TARGET};
                }
                else
                {
                    return new SkillResult(){ SUCCESS = true, result = MD.ActionResult.CAST};
                }
            }                
            case MD.SkillType.TANK:
            {
                var target = Player.CurrentTarget;
                if(target == null)
                    return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.INVALID_TARGET};
                var status = (AdversaryStatus)target.Status;
                if(status == null)
                {
                    return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.INVALID_TARGET};
                }
                else
                {
                    return new SkillResult(){ SUCCESS = true, result = MD.ActionResult.CAST};
                }             
            }
            default:
            {
                return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.INVALID_SETUP};
            }
        }
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