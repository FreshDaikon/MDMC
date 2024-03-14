using Godot;
using Mdmc.Code.Game.Combat;
using Mdmc.Code.Game.Realization;


[GlobalClass]
public partial class InstantController : ActionController
{
    public override SkillResult ActivateAction()
    {
        // Instant Cast can just trigger right away:
        Rpc(nameof(RealizeActionTrigger));
        foreach(var action in skillActions)
        {
            action.TriggerAction();
        }
        EmitSignal(SignalName.ActionsTriggered);
        return new SkillResult(){
            SUCCESS = true,
            result = Mdmc.Code.System.MD.ActionResult.CAST
        };
    }

    public override SkillResult CanActivate()
    {
        foreach(var action in skillActions)
        {
            if(!action.CanTrigger()) return new SkillResult(){ 
                SUCCESS = false,
                result = Mdmc.Code.System.MD.ActionResult.INVALID_TARGET };
        }
        return new SkillResult(){ SUCCESS = true, result = Mdmc.Code.System.MD.ActionResult.CAST };
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