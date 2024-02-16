using Godot;
using SmartFormat;
using Daikon.System;

namespace Daikon.Game;

public partial class Skill : Node
{
    //
    // Serialized Properties:
    //
    [Export]
    public Texture2D Icon;
    [Export]
    public string SkillName = "Some Skill Name";
    [Export(PropertyHint.MultilineText)]
    public string SkillDescription = "Usable Tags : {potency}, {type}";    
    
    // Up for Debate atm:
    //
    [ExportGroup("Skill Options:")]
    [Export]
    public bool IsUniversalSkill = false;
    [Export]
    public MD.SkillTimerType TimerType;
    [Export]
    public int BasePotency = 100;
    public int AdjustedPotency = 100;
    [Export]
    public float Range = 10f;
    [Export]
    public float Cooldown = 1f;
    [ExportCategory("Action Type")]
    [Export]
    public MD.SkillActionType ActionType;
    [ExportCategory("Casting Skill")]
    [Export]
    public bool CanMove = false;
    [Export]
    public float CastTime = 0f;
    [ExportCategory("Channeling SKill")]
    [Export]
    public float ChannelTime = 0f;
    [Export]
    public float TickRate = 1f;

    [ExportGroup("Standard Realizations")]
    [Export(PropertyHint.File)]
    public string RealizeCastPath;
    [Export(PropertyHint.File)]
    public string RealizeFinishedPath;
    [Export(PropertyHint.File)]
    public string RealizeSkillPath;

    //Class Internals:
    public RealizationObject CurrentRealization;
    public MD.SkillType SkillType;
    public PlayerEntity Player;
    public ulong StartTime;

    // Internal ID for Loading
    [ExportGroup("Internal")]
    [Export]
    private DataID ID;
    public int Id
    {
        get { return ID.Id; }
    }
    // Internal Time Keeping.
    private bool isCasting;
    private ulong startCastTime;
    private bool isChanneling;
    private ulong startChannelTime;
    //Time Keeping continued:
    private int lastLapse = 0;
    private int ticks = 0;

    public string GetDescription()
    {
        var descData = new { 
            potency = BasePotency.ToString(), 
            category = SkillType.ToString(),
            timer = TimerType.ToString()
        };
        var form = Smart.Format(SkillDescription, descData);
        return form;
    }


    public override void _Ready()
    {
        CallDeferred(nameof(InitSkill));
    }

    private void InitSkill()
    {
        MD.Log("Setting up skill dependencies");
        Player = GetParent().GetParent<SkillSlot>().Player;
        MD.Log("Skill Player : " + Player.Name);
        if(!IsUniversalSkill)
        {
            SkillType = GetParent().GetParent<SkillSlot>().SlotSkillType;
        }
    }
    public void Reset()
    {
        isCasting = false;
        isChanneling = false;
        lastLapse = 0;
        StartTime = GameManager.Instance.ServerTick - (ulong)(Cooldown * 1000f);
        Rpc(nameof(SyncCooldown), StartTime);
    }
    public SkillResult TriggerSkill()
    {
        if(!Multiplayer.IsServer())
            return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.NOT_SERVER };

        if(!CheckCooldown())
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
            StartTime = GameManager.Instance.ServerTick;
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
            Rpc(nameof(RealizeCast));
            Player.Arsenal.StartCasting(this);
            startCastTime = GameManager.Instance.ServerTick;
            isCasting = true;
            return new SkillResult(){ result = MD.ActionResult.CAST };
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
            startChannelTime = GameManager.Instance.ServerTick;
            isChanneling = true;
            return new SkillResult(){ result = MD.ActionResult.CAST };            
        }
        return check;        
    }
    
    public void InteruptCast()
    {
        isCasting = false;
        Rpc(nameof(CancelRealization));        
    }
  
    public void InteruptChannel()
    {
        Rpc(nameof(CancelRealization));
        isChanneling = false;
        ticks = 0;
        lastLapse = 0;
    }    
    
    public override void _PhysicsProcess(double delta)
    {
        if(!Multiplayer.IsServer())
            return;

        if(isCasting)
        {
            float lapsed = (GameManager.Instance.ServerTick - startCastTime) / 1000f;
            if(lapsed > CastTime)
            {
                Rpc(nameof(RealizeFinished));
                isCasting = false;
                if(Cooldown > 0f)
                {
                    StartTime = GameManager.Instance.ServerTick;
                    Rpc(nameof(SyncCooldown), StartTime);
                }
                var Result = TriggerResult();
                Player.Arsenal.FinishCasting(Result);
            }
        }
        if(isChanneling)
        {
            float lapsed = (GameManager.Instance.ServerTick - startChannelTime) / 1000f;
            float scaled = lapsed * TickRate;
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
                    StartTime = GameManager.Instance.ServerTick;
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

    public bool CheckCooldown()
    {
        if(Cooldown <= 0f)
            return true;
        else
        {
            if(((GameManager.Instance.ServerTick - StartTime) / 1000f) < Cooldown)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncCooldown(ulong time)
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
        RealizationObject realization = DataManager.Instance.GetRealizationObjectFromPath(RealizeCastPath);
        realization.Spawn(Player.Controller.Position);
        CurrentRealization = realization;
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RealizeFinished()
    {       
        RealizationObject realization = DataManager.Instance.GetRealizationObjectFromPath(RealizeFinishedPath);
        realization.Spawn(Player.Controller.Position);
    }    
    public virtual SkillResult TriggerResult()
    {
        GD.PrintErr("Please Override this implementation! (sorry, cant do abstraction with nodes haha!)");
        return new SkillResult(){ SUCCESS = true, result = MD.ActionResult.CAST };
    }
    public virtual SkillResult CheckSkill()
    {
        GD.PrintErr("Please Override this implementation! (sorry, cant do abstract haha!)");
        return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.INVALID_TARGET }; 
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public virtual void SkillRealization(int value, int type)
    {
        // VALUE : value of the result of the skill (damage, heal etc value)
        // TYPE : type of the result (DAMAGE, HEAL, etc)
        //Here we do skill specific realizations:
    }
}