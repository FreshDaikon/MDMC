using System.Linq;
using Daikon.Client;
using Daikon.Game.General;
using Daikon.Game.Realizations;
using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class SinglePotency : Skill
{
    public ChaseTargetRealizationData OnSkillCastRealization;
    
    
    public override SkillResult TriggerResult()
    {
        if(!Multiplayer.IsServer())
            return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.NOT_SERVER };

        if(IsUniversalSkill)
        {
            return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.INVALID_SETUP };
        }

        var workingValue = AdjustedPotency;
        
        if (Effects != null)
        {
            var potency = Effects.Where(e => e.Type == EffectData.EffectType.Potency);
            if (potency.Any())
            {
                GD.Print("The value of the sum is: " + potency.Sum(p => p.Value));
                GD.Print("The sum of all the potency effects are:" + ( AdjustedPotency * potency.Sum(p => p.Value))); 
                workingValue = (int)( AdjustedPotency * potency.Sum(p => p.Value));
                GD.Print("New working value : " + workingValue);
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
                Rpc(nameof(OnSkillCast), dmgDone);
                return new SkillResult(){ SUCCESS = true, result = MD.ActionResult.CAST};
            };
            case MD.SkillType.HEAL:
            {
                var target = Player.CurrentFriendlyTarget.Status;
                var healDone = target.InflictHeal(workingValue, Player);
                var message = new CombatMessage()
                {
                    Caster = int.Parse(Player.Name),
                    Target = int.Parse(Player.CurrentFriendlyTarget.Name),
                    Value = healDone,
                    MessageType = MD.CombatMessageType.HEAL,
                    Effect = "X healed Y"
                };
                CombatManager.Instance.AddCombatMessage(message);
                Rpc(nameof(OnSkillCast), healDone);
                return new SkillResult(){ SUCCESS = true, result = MD.ActionResult.CAST};
            }                
            case MD.SkillType.TANK:
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
                Rpc(nameof(OnSkillCast), dmgDone);
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

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void OnSkillCast(int value)
    {
        if(Multiplayer.IsServer()) return;
        if(OnSkillCastRealization == null) return;
        
        GD.Print("We are supposed to spawn the realization here!");

        var target = SkillType == MD.SkillType.HEAL ? Player.CurrentFriendlyTarget.Controller : Player.CurrentTarget.Controller;
        var color = MD.GetSkillTypeColor(SkillType);
        
        var newRealization = (ChaseTargetRealization)OnSkillCastRealization.GetRealization();
        newRealization.SetData(Player.Controller.GlobalPosition, target, 5f, 10f);
        newRealization.Spawn();

        newRealization.OnRealizationEnd += () =>
        {
            UIHUDMain.Instance?.SpawnDamageNumber(value.ToString(), color, newRealization.GlobalPosition);
        };
    }
    
    
}