using System.Linq;
using Godot;
using Daikon.Helpers;
using System.Collections.Generic;
using System;

namespace Daikon.Game;

public partial class PlayerArsenal: Node
{
    [Export]
    public float BaseGcd { get; private set; } = 1.5f;
    
    public PlayerEntity Player;
    public bool IsCasting = false;
    public double CastingStartTime;
    public double CastingTime;
    public bool IsChanneling = false;
    public double ChannelingStartTime;
    public double ChannelingTime;
    public Skill CastingSkill;
    public Skill ChannelingSkill;
    public double GCDStartTime = 0;
    
    public EffectStack Stack { get; private set; }
    public Skill LastSkill { get; set; }
    
    
    bool hasBeenInit = false;
    bool hasBeenInitLocal = false;

    #region Godot 
    public override void _Ready()
    {
        //skillContainers = GetNode("%SkillContainers");
        Player = GetParent<PlayerEntity>();
        Stack = new EffectStack(this);
    }

    public override void _Process(double delta)
    {
        if (hasBeenInitLocal) return;
        if (!GameManager.Instance.IsGameRunning()) return;
        if(!Multiplayer.IsServer())
        {
            GD.Print("Ask the server to sync us pretty please!");
            RpcId(1, nameof(RequestStartupSync));
            hasBeenInitLocal = true;
        }
        else
        {
            Player.Status.KnockedOut += () =>
            {
                if(IsCasting) TryInteruptCast();
                if(IsChanneling) TryInterruptChanneling();
            };
            hasBeenInitLocal = true;
        }
    }

    #endregion

    #region Setup:

    private void SetupDebug()
    {
        var containers = DataManager.Instance.GetAllSkillContainers();
        var skills = DataManager.Instance.GetAllSkills();
        var slots = new MD.ContainerSlot[]{
            MD.ContainerSlot.Main, 
            MD.ContainerSlot.Right,
            MD.ContainerSlot.Left};
        foreach(var slot in slots)
        {
            var firstContainer = containers[0];
            GD.Print("Set Container : " + firstContainer.Name + " To Container: " + slot);
            var firstSkill = skills[0];
            SetSkillContainer(firstContainer.Id, slot);
            Rpc(nameof(SyncSkillContainer), firstContainer.Id, (int)slot);
            
            SetSkill(firstSkill.Id, slot, 0);
            SetSkill(firstSkill.Id, slot, 1);
            SetSkill(firstSkill.Id, slot, 2);
            SetSkill(firstSkill.Id, slot, 3);

            Rpc(nameof(SyncSkill), firstSkill.Id, (int)slot, 0);
            Rpc(nameof(SyncSkill), firstSkill.Id, (int)slot, 1);
            Rpc(nameof(SyncSkill), firstSkill.Id, (int)slot, 2);
            Rpc(nameof(SyncSkill), firstSkill.Id, (int)slot, 3);
        }
        hasBeenInit = true;
    }  

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestStartupSync()
    {
        var requester = Multiplayer.GetRemoteSenderId();
        if(hasBeenInit)
        {
            if(GetChildCount() > 0)
            {
                var containers = GetChildren()
                    .Where(c => c is SkillContainer)
                    .Cast<SkillContainer>()
                    .ToList();
                foreach(var container in containers)
                {                    
                    RpcId(requester, nameof(SyncSkillContainer), container.Data.Id, (int)container.AssignedSlot);
                    RpcId(requester,nameof(SyncSkill), container.GetSkill(0).Data.Id, (int)container.AssignedSlot, 0);
                    RpcId(requester,nameof(SyncSkill), container.GetSkill(1).Data.Id, (int)container.AssignedSlot, 1);
                    RpcId(requester,nameof(SyncSkill), container.GetSkill(2).Data.Id, (int)container.AssignedSlot, 2);
                    RpcId(requester,nameof(SyncSkill), container.GetSkill(3).Data.Id, (int)container.AssignedSlot, 3);
                }
            }
        }
        else
        {
            SetupDebug();
        }
    }

    public void ResetArsenal()
    {
        GCDStartTime = GameManager.Instance.GameClock - BaseGcd;
        Rpc(nameof(SyncGCD), GCDStartTime);
        IsCasting = false;
        IsChanneling = false;
        CastingSkill = null;
        ChannelingSkill = null;
    }

    public SkillContainer GetSkillContainer(MD.ContainerSlot containerSlot)
    {
        if(GetChildCount() > 0)
        {
            var container = GetChildren().Where(s => s is SkillContainer).Cast<SkillContainer>().ToList().Find(x => x.AssignedSlot == containerSlot);
            return container;
        }
        return null;
    }

    public void SetSkillContainer(int id, MD.ContainerSlot containerSlot) 
    {
        var newContainer = DataManager.Instance.GetSkillContainerInstance(id);
        if(newContainer == null)
        {
            GD.Print("Skill Container with ID: " + id + " does not exist.");
            return;
        }
        if(GetChildCount() > 0)
        {
            var current = GetChildren().Where(s => s is SkillContainer).Cast<SkillContainer>().ToList().Find(x => x.AssignedSlot == containerSlot);
            current?.Free();
        }
        // Replace it with the new one:
        newContainer.Name = containerSlot.ToString();
        newContainer.AssignedSlot = containerSlot;
        AddChild(newContainer); 

        newContainer.InitSkills();
    }    
    
    public Skill GetSkill(MD.ContainerSlot containerSlot, int slot)
    {
        var container = GetSkillContainer(containerSlot);
        if(container != null)
        {
            return container.GetSkill(slot);
        }
        return null;
    }

    public void SetSkill(int id, MD.ContainerSlot containerSlot, int slot)
    {
        var container = GetSkillContainer(containerSlot);
        if(container != null)
        {
            container.SetSkill(id, slot);
        }
        else
        {
            GD.Print("Container was null... bullockes!");
        }
    }

    public void TrySetContainer(int id, int slot)
    {
        RpcId(1, nameof(RequestContainerChange), id, slot);
    }

    public void TrySetSkill(int id, int containerSlot, int slot)
    {
        RpcId(1, nameof(RequestSkillChange), id, containerSlot, slot);
    }    
    
    #endregion
    
    #region SERVER_CALLS

    public SkillResult TriggerSkill(MD.ContainerSlot containerSlot, int index)
    { 
        var container = GetSkillContainer(containerSlot);
        if(container != null)
        {
            if(IsCasting)
            {
                return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.IS_CASTING };
            }
            if(IsChanneling)
            {
                return container.TriggerSkill(index); 
            }
            if(container.IsSkillOGCD(index))
            {
                GD.Print("Try Trigger OGCD Skill:");
                return container.TriggerSkill(index);
            }            
            
            var lapsed = GameManager.Instance.GameClock - GCDStartTime;
            var modGCD = BaseGcd; 
            if(lapsed < modGCD)
            {
                return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.ON_COOLDOWN };
            }
            else
            {
                var result = container.TriggerSkill(index);
                if(result.SUCCESS)
                {
                    GD.Print("We triggered and it was success.");
                    Rpc(nameof(SyncGCD), GameManager.Instance.GameClock);
                    GCDStartTime = GameManager.Instance.GameClock;
                    return result;
                }
                else
                {
                    GD.Print("We triggered, but it was not success");
                    return result;
                }
            }      
        }
        return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.ERROR };
    }

    public void StartCasting(Skill caster)
    {
        IsCasting = true;
        if(caster.TimerType == MD.SkillTimerType.GCD)
        {
            GCDStartTime = GameManager.Instance.GameClock;
            Rpc(nameof(SyncGCD), GameManager.Instance.GameClock);
        }
        CastingStartTime = GameManager.Instance.GameClock;
        CastingTime = caster.CastTime;
        Rpc(nameof(SyncCasting), IsCasting, CastingStartTime, CastingTime);
        CastingSkill = caster;
    }

    public void StartChanneling(Skill caster)
    {
        IsChanneling = true;
        if(caster.TimerType == MD.SkillTimerType.GCD)
        {
            GCDStartTime = GameManager.Instance.GameClock;
            Rpc(nameof(SyncGCD), GameManager.Instance.GameClock);
        }
        ChannelingStartTime = GameManager.Instance.GameClock;
        ChannelingTime = caster.ChannelTime;
        ChannelingSkill = caster;
        Rpc(nameof(SyncChanneling), IsChanneling, ChannelingStartTime, ChannelingTime);
    }

    public void FinishCasting(SkillResult result)
    {
        GD.Print("Skill result after channeling:" + result.result);
        CastingSkill = null;
        IsCasting = false;
        Rpc(nameof(SyncCasting), IsCasting, CastingStartTime, CastingTime);
    }

    public void FinishChanneling(SkillResult result)
    {
        GD.Print("Skill result after channeling:" + result.result);
        ChannelingSkill = null;
        IsChanneling = false;
        Rpc(nameof(SyncChanneling), IsChanneling, ChannelingStartTime, ChannelingTime);
    }

    public void TryInteruptCast()
    {
        if (!IsCasting || CastingSkill.CanMove) return;
        
        if(CastingSkill.TimerType == MD.SkillTimerType.GCD)
        { 
            GCDStartTime = GCDStartTime - BaseGcd;
            Rpc(nameof(SyncGCD), GCDStartTime);
        }
        CastingSkill.InterruptCast();
        CastingSkill = null;
        IsCasting = false;
        Rpc(nameof(SyncCasting), IsCasting, CastingStartTime, CastingTime);
        var result = new SkillResult() { result = MD.ActionResult.CASTING_INTERUPTED };
    }

    public void TryInterruptChanneling()
    {
        
        if (!IsChanneling || ChannelingSkill.CanMove) return;
        
        ChannelingSkill.InterruptChannel();
        ChannelingSkill = null;
        IsChanneling = false;
        Rpc(nameof(SyncCasting), IsCasting, CastingStartTime, CastingTime);
        FinishChanneling(new SkillResult(){  } );
    }
    #endregion
       

    #region RPC_CALLS

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestContainerChange(int id, int slot)
    {
        SetSkillContainer(id, (MD.ContainerSlot)slot);
        Rpc(nameof(SyncSkillContainer), id, slot);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestSkillChange(int id, int containerSlot, int slot)
    {
        SetSkill(id, (MD.ContainerSlot)containerSlot, slot);
        Rpc(nameof(SyncSkill), id, containerSlot, slot);
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncSkillContainer(int id, int containerSlot)
    {
        GD.Print("Adding skillContainer on client for " + containerSlot);
        SetSkillContainer(id, (MD.ContainerSlot)containerSlot);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncSkill(int id, int containerSlot, int slot)
    {
        GD.Print("Adding skill on client");
        SetSkill(id, (MD.ContainerSlot)containerSlot, slot);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncGCD(double time)
    {
        GCDStartTime = time;
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncCasting(bool casting, double start, double time)
    {
        IsCasting = casting;
        CastingStartTime = start;
        CastingTime = time;
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncChanneling(bool channeling, double start, double time)
    {
        IsChanneling = channeling;
        ChannelingStartTime = start;
        ChannelingTime = time;
    }
    #endregion

    #region Utility
    // Various utility functions for both Server And Client:
   
    public SkillResult CanCast(MD.ContainerSlot containerSlot, int slot)
    {
        var result = new SkillResult() { SUCCESS = false, result= MD.ActionResult.ERROR };
        if(IsCasting)
        {
            result.result = MD.ActionResult.IS_CASTING;
            return result;
        }
        if(IsChanneling)
        {
            GD.Print("This channeling Triggered!");
            result.result = MD.ActionResult.IS_CHANNELING;
            return result;
        }
        if(IsArsenalOnCD())
        {
            result.result = MD.ActionResult.ON_COOLDOWN;
            return result;
        }  
        var container = GetSkillContainer(containerSlot);
        if(container == null)
        {
            result.result = MD.ActionResult.ERROR;
            return result;
        }
        var skill = container.GetSkill(slot);
        if(skill == null)
        {
            result.result = MD.ActionResult.ERROR;
            return result;
        }
        if(skill.IsOnCooldown())
        {            
            result.result = MD.ActionResult.ON_COOLDOWN;
            return result;
        }
        var quickCheck = skill.CheckSkill();
        if(!quickCheck.SUCCESS)
        {
            return result;
        }
        result.SUCCESS = true;
        result.result = MD.ActionResult.CAST;
        return result;
    }
    

    public bool IsArsenalOnCD()
    {
        var lapsed = GameManager.Instance.GameClock - GCDStartTime;
        var modGCD = BaseGcd;
        if(lapsed < modGCD)
        {
            return true;
        }
        return false;
    }

    public float GetArsenalGCD()
    {
        var modGCD = BaseGcd;
        return modGCD;
    }    

    public float[] GetArsenalSkillWeights()
    {
        float[] weights = new float[3];
        
        if(GetChildCount() > 0)
        {
            var containers = GetChildren()
                .Where(c => c is SkillContainer)
                .Cast<SkillContainer>()
                .ToList();

            foreach(var container in containers)
            {                
                var skillSlots = container.SkillSlots.ToList();
                foreach(var slot in skillSlots)
                {
                    var skill = container.GetSkill(skillSlots.IndexOf(slot));
                    if(skill == null || skill.IsUniversalSkill)
                    {
                        weights[0] += 0.333f;
                        weights[1] += 0.333f;
                        weights[2] += 0.333f;
                        continue;
                    }
                    switch(skill.SkillType)
                    {
                        case MD.SkillType.DPS:
                            weights[0] += 1f;
                            break;  
                        case MD.SkillType.HEAL:
                            weights[1] += 1f;
                            break;                                              
                        case MD.SkillType.TANK:
                            weights[2] += 1f;
                            break;   
                    }
                }                
            }       
            return weights;
        }
        else
        {
            return weights;
        }
    } 
    public float[] GetWeightedTotal(float[] weights)
    {
        var list = weights.ToList();
        float[] totals = new float[3];
        foreach(float value in list)
        {
            totals[list.IndexOf(value)] = Mathf.Remap(value, 0, 12, 0.1f, 0.9f); 
        }
        return totals;
    }
    #endregion
}