using System.Linq;
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

        var workingValue = AdjustedPotency;
        
        if (Effects != null)
        {
            var potency = Effects.Where(e => e.Type == Effect.EffectType.Potency);
            if (potency.Any())
            {
                GD.Print("The sum of all the potency effects are:" + ( AdjustedPotency * potency.Sum(p => p.Value))); 
                workingValue = (int)( AdjustedPotency * potency.Sum(p => p.Value));
            }
        }
        
        switch(SkillType)
        {
            case MD.SkillType.DPS:
            {
                var target = (AdversaryStatus)Player.CurrentTarget.Status;
                var dmgDone = target.InflictDamage(workingValue, Player);
                target.InflictThreat((int)(dmgDone * ThreatMultiplier), Player);
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
                target.InflictThreat((int)(dmgDone * ThreatMultiplier), Player);
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
            GD.Print("Yah player not init on this fucker..");
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
                var target = Player.CurrentFriendlyTarget;
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
        var realization = RealizeOnSkill.GetRealization();
        // Expect this realization to be the damage number one so pass the extra data
        var damageNumberInfo = new { Value = value, Type = type};
        var target = SkillType == MD.SkillType.HEAL ? Player.CurrentFriendlyTarget.Controller : Player.CurrentTarget.Controller;
        realization.Spawn(Player.Controller.GlobalPosition, target, 10f, 2f, damageNumberInfo);
    }
}