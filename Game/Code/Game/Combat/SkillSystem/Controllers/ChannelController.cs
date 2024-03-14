using Godot;
using Mdmc.Code.Game;
using Mdmc.Code.Game.Combat;
using Mdmc.Code.Game.Realization;


[GlobalClass]
public partial class ChannelController : ActionController
{    

    [Export] public float BaseChannelTime = 2.5f;
    [Export] public float TickRate;
    [Export] public bool CanMove = false;

    [Export] private PackedScene _channelFinishedRealizationScene;

    public bool IsChanneling { get; private set;}
    public double CurrentChannelTime { get; private set;}
    public double StartChannelTime { get; private set;}
    public double Lapsed { get; private set;}
    public double LastLapse { get; private set;}
    public int Ticks { get; private set;}

    [Signal] public delegate void FinishedChannelingEventHandler();
    
    public override void _PhysicsProcess(double delta)
    {
        if(IsChanneling)
        {
            Lapsed = GameManager.Instance.GameClock - StartChannelTime;
            var scaled = Lapsed * TickRate;
            if((int)scaled > LastLapse)
            {
                Ticks += 1;
                LastLapse = (int)scaled;        
                TriggerActions();
            }
            if(Lapsed > CurrentChannelTime)
            {
                Ticks = 0;
                LastLapse = 0;
                IsChanneling = false;
                EmitSignal(SignalName.ActionsTriggered);
                EmitSignal(SignalName.FinishedChanneling);
            }
        }
    }


    public override SkillResult ActivateAction()
    {
        StartChannelTime = GameManager.Instance.GameClock;
        IsChanneling = true;
        CurrentChannelTime = BaseChannelTime;
        Rpc(nameof(RealizeActionTrigger));
        return new SkillResult()
        {
            SUCCESS = true,
            result = Mdmc.Code.System.MD.ActionResult.START_CHANNELING,
            extraData = new 
            {
                controller = this,
                startTime = StartChannelTime,
                channelTime = CurrentChannelTime
            }
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

    public void Interrupt(bool wasMove)
    {
        if(!IsChanneling) return;

        if(wasMove)
        {
            IsChanneling = CanMove;
            return;
        }
        IsChanneling = false;
    }

    public void TriggerActions()
    {
        foreach(var action in skillActions)
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
        if(_channelFinishedRealizationScene == null) return;

        var builder = RealizationManager.Instance.CreateRealizationBuilder();
        var real = builder.New(_channelFinishedRealizationScene, 2)
            .WithStartPosition(_skill.Arsenal.Player.Controller.GlobalPosition)
            .Spawn();            
    }
}