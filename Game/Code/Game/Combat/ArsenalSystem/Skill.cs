using Godot;
using SmartFormat;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class Skill : Node
{
    // Runtime Data:
    public bool IsUniversalSkill = false;
    public MD.SkillTimerType TimerType;
    public int BasePotency = 100;
    public int AdjustedPotency = 100;
    public float Range = 10f;
    public float Cooldown = 1f;
    public MD.SkillActionType ActionType;
    public bool CanMove = false;
    public float CastTime = 0f;
    public float ChannelTime = 0f;
    public float TickRate = 1f;
    public float ThreatMultiplier = 1f;
    // Maintinece:
    public int AssignedSlot = -1;
    // Realizations 
    public RealizationObject RealizeOnCast;
    public RealizationObject RealizeOnFinish;    
    public RealizationObject RealizeOnSkill;

    //Class Internals:
    public SkillObject Data;
    public Realization CurrentRealization;
    public MD.SkillType SkillType;
    public PlayerEntity Player;
    public double StartTime;

    // Internal Time Keeping.
    private bool isCasting;
    private double startCastTime;
    private bool isChanneling;
    private double startChannelTime;
    //Time Keeping continued:
    private int lastLapse = 0;
    private int ticks = 0;

    public void InitSkill()
    {
        if(Cooldown > 0f)
        {
            StartTime = Mathf.Max(GameManager.Instance.GameClock + Cooldown, 0);
        }
    }
    public void Reset()
    {
        isCasting = false;
        isChanneling = false;
        lastLapse = 0;
        StartTime = GameManager.Instance.GameClock - Cooldown;
        Rpc(nameof(SyncCooldown), StartTime);
    }
    public SkillResult TriggerSkill()
    {
        if(!Multiplayer.IsServer())
            return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.NOT_SERVER };

        if(IsOnCooldown())
        {
            return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.ON_COOLDOWN };
        }
        if(ActionType == MD.SkillActionType.CAST)
        {            
            return StartCast();
        }
        if(ActionType == MD.SkillActionType.CHANNEL)
        {
            return StartChannel();
        }
        var check = CheckSkill();
        if(!check.SUCCESS)
        {
            return check;
        }
        if(Cooldown > 0f)
        {
            StartTime = GameManager.Instance.GameClock;
            Rpc(nameof(SyncCooldown), StartTime);
        }
        Rpc(nameof(RealizeFinished));
        return TriggerResult();        
    }

    public SkillResult StartCast()
    {
        var check = CheckSkill();
        if(check.SUCCESS)
        {
            GD.Print("Start Casting!");
            Rpc(nameof(RealizeCast));
            Player.Arsenal.StartCasting(this);
            startCastTime = GameManager.Instance.GameClock;
            isCasting = true;
            return new SkillResult(){SUCCESS = true, result = MD.ActionResult.CAST };
        }
        return check;        
    }
    
    public SkillResult StartChannel()
    {
        var check = CheckSkill();
        if(check.SUCCESS)
        {
            Rpc(nameof(RealizeCast));
            Player.Arsenal.TryInteruptChanneling();
            Player.Arsenal.StartChanneling(this);
            ticks = 0;
            lastLapse = 0;
            startChannelTime = GameManager.Instance.GameClock;
            isChanneling = true;
            return new SkillResult(){ result = MD.ActionResult.CAST };            
        }
        return check;        
    }
    
    public void InteruptCast()
    {
        GD.Print("try interupt casting.");
        if(isCasting)
        {
            isCasting = false;        
            Rpc(nameof(CancelRealization));        
        }
    }
  
    public void InteruptChannel()
    {
        if(isChanneling)
        {
            Rpc(nameof(CancelRealization));
            isChanneling = false;
            ticks = 0;
            lastLapse = 0;
        }
    }    
    
    public override void _PhysicsProcess(double delta)
    {
        if(!Multiplayer.IsServer())
            return;
        if(isCasting)
        {
            double lapsed = GameManager.Instance.GameClock - startCastTime;
            if(lapsed > CastTime)
            {
                Rpc(nameof(RealizeFinished));
                isCasting = false;
                if(Cooldown > 0f)
                {
                    StartTime = GameManager.Instance.GameClock;
                    Rpc(nameof(SyncCooldown), StartTime);
                } 
                var Result = TriggerResult();
                Player.Arsenal.FinishCasting(Result);
            }
        }
        if(isChanneling)
        {
            double lapsed = GameManager.Instance.GameClock - startChannelTime;
            double scaled = lapsed * TickRate;
            if((int)scaled > lastLapse)
            {
                ticks += 1;
                lastLapse = (int)scaled;        
                var Tick = TriggerResult();
                MD.Log("Channel Skill Tick : " + Tick.result );
            }
            if(lapsed > ChannelTime)
            {
                if(Cooldown > 0f)
                {
                    StartTime = GameManager.Instance.GameClock;
                    Rpc(nameof(SyncCooldown), StartTime);
                }
                Rpc(nameof(RealizeFinished));
                ticks = 0;
                lastLapse = 0;
                isChanneling = false;
                Player.Arsenal.FinishChanneling(new SkillResult(){ SUCCESS= true, result=MD.ActionResult.CHANNELING_FINISHED });
            }
        }
    }

    public bool IsOnCooldown()
    {
        if(Cooldown <= 0f)
            return false;
        else
        {
            return (GameManager.Instance.GameClock - StartTime) <= Cooldown;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncCooldown(double time)
    {
        StartTime = time;
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void CancelRealization()
    {
        if(CurrentRealization != null)
        {
            CurrentRealization.Kill();
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RealizeCast()
    {
        var realization = RealizeOnCast.GetRealization();
        realization.Spawn(Player.Controller.GlobalPosition, 2f);
        CurrentRealization = realization;
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RealizeFinished()
    {       
        var realization = RealizeOnFinish.GetRealization();
        realization.Spawn(Player.Controller.Position, 2f);
    }    
    public virtual SkillResult TriggerResult()
    {
        GD.PrintErr("Please Override this implementation!");
        return new SkillResult(){ SUCCESS = true, result = MD.ActionResult.CAST };
    }

    public virtual SkillResult CheckSkill()
    {
        GD.PrintErr("Please Override this implementation!");
        return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.INVALID_TARGET }; 
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public virtual void SkillRealization(int value, int type)
    {    }
    
}