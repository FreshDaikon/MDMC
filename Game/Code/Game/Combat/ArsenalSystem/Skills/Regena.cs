using Godot;
using Daikon.System;

namespace Daikon.Game;

public partial class Regena : Skill
{   
    [Export(PropertyHint.File)]
    public string ModifierPath;
    private int modId;

    private Modifier modInstance;

    public override void _Ready()
    {        
        //Slightly Messy Setup:
        var temp = (PackedScene)ResourceLoader.Load(ModifierPath);
        var instance = temp.Instantiate<Modifier>();
        modId = instance.Id;
        instance.QueueFree();
        base._Ready();
    }

    public override SkillResult TriggerResult()
    {
        if(!Multiplayer.IsServer())
            return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.NOT_SERVER };

        var target = Player.CurrentTarget;
        var mod = DataManager.Instance.GetModifier(modId);
        if(mod != null)
        {
            var result = target.Modifiers.AddModifier(mod);
            if(result.SUCCESS)
            {
                modInstance = mod;
                var message = new CombatMessage()
                {
                    Caster = int.Parse(Player.Name),
                    Target = int.Parse(Player.CurrentTarget.Name),
                    MessageType = MD.CombatMessageType.EFFECT,
                    Effect = "Applied MOD to TARGET"
                };
                CombatManager.Instance.AddCombatMessage(message);
                Rpc(nameof(SkillRealization), message.Value, (int)message.MessageType);
                return result;
            }
            else
            {
                mod.QueueFree();
                return result;
            }
        }    
        return new SkillResult() { SUCCESS = false, result = MD.ActionResult.ERROR};    
    }
    public override SkillResult CheckSkill()
    {
        var target = Player.CurrentTarget;
        if(target == null)
            return new SkillResult() { SUCCESS = false, result = MD.ActionResult.INVALID_TARGET };
        else
        {
            return new SkillResult() { SUCCESS = true, result = MD.ActionResult.CAST };
            //Here we get the damage number version!        
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