using Godot;
using Mdmc.Code.Game.RealizationSystem;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Combat.SkillSystem.Controllers;

[GlobalClass]
public partial class InstantController : ActionController
{
    public override SkillResult ActivateAction()
    {
        Rpc(nameof(RealizeActionTrigger));
        foreach(var action in SkillActions)
        {
            action.TriggerAction();
        }
        EmitSignal(SignalName.ActionsTriggered);
        return new SkillResult
        {
            SUCCESS = true,
            result = MD.ActionResult.CAST
        };
    }

    public override SkillResult CanActivate()
    {
        foreach(var action in SkillActions)
        {
            if(!action.CanTrigger()) return new SkillResult
            { 
                SUCCESS = false,
                result = MD.ActionResult.INVALID_TARGET };
        }
        return new SkillResult { SUCCESS = true, result = MD.ActionResult.CAST };
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RealizeActionTrigger()
    {
        if(_realizationScene == null ) return;
        
        var builder = RealizationManager.Instance.CreateRealizationBuilder();
        var real = builder.New(_realizationScene, 2)
            .WithStartPosition(_skill.Arsenal.Player.Controller.GlobalPosition)
            .Spawn();            
    }
}