using Godot;
using Mdmc.Code.Game.RealizationSystem;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Combat.SkillSystem.Controllers;

[GlobalClass]
public partial class CastController : ActionController
{        
    [Export] public float BaseCastTime = 2.5f;
    [Export] public bool CanMove;

    [Export] private PackedScene _castFinishedRealization;

    public bool IsCasting { get; private set; }
    public double CurrentCastTime { get; private set; }
    public double StartCastTime { get; private set; }
    public double Lapsed { get; private set; }

    [Signal] public delegate void FinishedCastingEventHandler();

    public override void _PhysicsProcess(double delta)
    {
        if(IsCasting)
        {
            Lapsed = GameManager.Instance.GameClock - StartCastTime;
            if(Lapsed >CurrentCastTime)
            {
                IsCasting = false;
                CurrentCastTime = BaseCastTime;
                TriggerActions();
                EmitSignal(SignalName.FinishedCasting);
                EmitSignal(SignalName.ActionsTriggered);
                Rpc(nameof(RealizeCastFinishedTrigger));
            }
        }
    }

    public override SkillResult ActivateAction()
    {
        StartCastTime = GameManager.Instance.GameClock;
        IsCasting = true;
        CurrentCastTime = BaseCastTime;
        Rpc(nameof(RealizeActionTrigger));
        return new SkillResult
        {
            SUCCESS = true,
            result = MD.ActionResult.START_CASTING,
            extraData = new 
            {
                controller = this,
                startTime = StartCastTime,
                castTime = CurrentCastTime
            }
        };
    }

    public override SkillResult CanActivate()
    {
        foreach(var action in SkillActions)
        {
            if(!action.CanTrigger()) 
                return new SkillResult
                { 
                    SUCCESS = false,
                    result = MD.ActionResult.INVALID_TARGET
                };
        }
        return new SkillResult { SUCCESS = true, result = MD.ActionResult.CAST };
    }

    public void Interrupt(bool wasMove)
    {
        if(!IsCasting) return;

        if(wasMove)
        {
            IsCasting = CanMove;
            return;
        }
        IsCasting = false;
    }

    private void TriggerActions()
    {
        foreach(var action in SkillActions)
        {
            action.TriggerAction();
        }
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RealizeActionTrigger()
    {
        if(_realizationScene == null) return;

        var builder = RealizationManager.Instance.CreateRealizationBuilder();
        var real = builder.New(_realizationScene, 2)
            .WithStartPosition(_skill.Arsenal.Player.Controller.GlobalPosition)
            .Spawn();            
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RealizeCastFinishedTrigger()
    {
        if(_castFinishedRealization == null) return;

        var builder = RealizationManager.Instance.CreateRealizationBuilder();
        var real = builder.New(_castFinishedRealization, 2)
            .WithStartPosition(_skill.Arsenal.Player.Controller.GlobalPosition)
            .Spawn();            
    }
}