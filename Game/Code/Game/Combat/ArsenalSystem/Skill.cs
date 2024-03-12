using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack;
using Mdmc.Code.Game.Data;
using Mdmc.Code.Game.Data.Decorators;
using Mdmc.Code.Game.Entity.Player;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Combat.ArsenalSystem;

public partial class Skill : Node
{
    // Runtime Data:
    public bool IsUniversalSkill = false;
    public MD.SkillTimerType TimerType;
    public bool RequiresResource = false;
    public int RequiredAmountOfResource = 1;
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
    
    public SkillContainer ParentContainer;
    public MD.ContainerSlot AssignedContainerSlot = MD.ContainerSlot.Main;
    public int AssignedSlot = -1;
    
    //Test :
    public EffectRuleData[] Rules;
    public List<Effect> Effects = new();

    //Class Internals:
    public SkillData Data;
    public MD.SkillType SkillType;
    public PlayerEntity Player;
    public double StartTime;

    // Internal Time Keeping.
    private bool _isCasting;
    private double _startCastTime;
    private bool _isChanneling;
    private double _startChannelTime;
    //Time Keeping continued:
    private int _lastLapse = 0;
    private int _ticks = 0;

    private float _adjustedCooldown;
    private float _adjustedCasttime;

    private List<ModTriggerEffct> _modEffects = new List<ModTriggerEffct>();
   
    // Signals :
    [Signal] public delegate void SkillTriggeredEventHandler(Skill skill);

    public void InitSkill()
    {
        if(Cooldown > 0f)
        {
            StartTime = Mathf.Max(GameManager.Instance.GameClock - Cooldown, 0);
        }
    }
    
    public void Reset()
    {
        _isCasting = false;
        _isChanneling = false;
        _lastLapse = 0;
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

        if(RequiresResource)
        {
            if(ParentContainer.GetCurrentResourceAmount() < RequiredAmountOfResource)
            {
                return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.NOT_ENOUGH_RESOURCE };
            }
        }

        switch (ActionType)
        {
            case MD.SkillActionType.CAST:
                return StartCast();
            case MD.SkillActionType.CHANNEL:
                return StartChannel();
            case MD.SkillActionType.INSTANT:
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
        EmitSignal(SignalName.SkillTriggered, this);
        if(RequiresResource)
        {
            ParentContainer.DecreaseResource(RequiredAmountOfResource);
        }
        return TriggerResult();        

    }

    public SkillResult StartCast()
    {
        var check = CheckSkill();
        if (!check.SUCCESS) return check;        
        GD.Print("Start Casting!");
        Player.Arsenal.StartCasting(this);
        _startCastTime = GameManager.Instance.GameClock;
        _isCasting = true;
        return new SkillResult(){SUCCESS = true, result = MD.ActionResult.CAST };
    }
    
    public SkillResult StartChannel()
    {
        var check = CheckSkill();
        if (!check.SUCCESS) return check;
        
        if(Player.Arsenal.ChannelingSkill != null) Player.Arsenal.TryInterruptChanneling();
        
        Player.Arsenal.StartChanneling(this);
        EmitSignal(SignalName.SkillTriggered, this);
        if(RequiresResource)
        {
            ParentContainer.DecreaseResource(RequiredAmountOfResource);
        }
        _ticks = 0;
        _lastLapse = 0;
        _startChannelTime = GameManager.Instance.GameClock;
        _isChanneling = true;
        return new SkillResult(){ result = MD.ActionResult.CAST };
    }
    
    public void InterruptCast()
    {
        if (!_isCasting) return;
        _isCasting = false;        
    }
  
    public void InterruptChannel()
    {
        if (!_isChanneling) return;
        _isChanneling = false;
        _ticks = 0;
        _lastLapse = 0;
    }    

    public void ApplyModifierEffect(ModTriggerEffct effect)
    {
        _modEffects.Add(effect);
    }

    
    public override void _PhysicsProcess(double delta)
    {
        if(!Multiplayer.IsServer())
            return;
        if(_isCasting)
        {
            var lapsed = GameManager.Instance.GameClock - _startCastTime;
            if(lapsed > CastTime)
            {
                _isCasting = false;
                if(Cooldown > 0)
                {
                    GD.Print("We should sync the cooldown...");
                    StartTime = GameManager.Instance.GameClock;
                    Rpc(nameof(SyncCooldown), StartTime);
                } 
                var Result = TriggerResult();
                if(RequiresResource)
                {
                    ParentContainer.DecreaseResource(RequiredAmountOfResource);
                }
                EmitSignal(SignalName.SkillTriggered, this);
                Player.Arsenal.FinishCasting(Result);
            }
        }
        if(_isChanneling)
        {
            var lapsed = GameManager.Instance.GameClock - _startChannelTime;
            var scaled = lapsed * TickRate;
            if((int)scaled > _lastLapse)
            {
                _ticks += 1;
                _lastLapse = (int)scaled;        
                var Tick = TriggerResult();
                GD.Print("Channel Skill Tick : " + Tick.result );
            }
            if(lapsed > ChannelTime)
            {
                if(Cooldown > 0f)
                {
                    StartTime = GameManager.Instance.GameClock;
                    Rpc(nameof(SyncCooldown), StartTime);
                }
                _ticks = 0;
                _lastLapse = 0;
                _isChanneling = false;
                Player.Arsenal.FinishChanneling(new SkillResult(){ SUCCESS= true, result=MD.ActionResult.CHANNELING_FINISHED });
            }
        }
    }

    public bool IsOnCooldown()
    {
        if(Cooldown <= 0)
            return false;
        return (GameManager.Instance.GameClock - StartTime) <= Cooldown;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncCooldown(double time)
    {
        StartTime = time;
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
    
}