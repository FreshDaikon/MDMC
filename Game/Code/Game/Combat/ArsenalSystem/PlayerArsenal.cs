using System.Linq;
using Godot;
using Mdmc.Code.Game.Entity.Player;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Combat.ArsenalSystem;

public partial class PlayerArsenal: Node
{
    [Export]
    public float BaseGcd { get; private set; } = 1.5f;
    
    // Player :
    public PlayerEntity Player { get; private set; }
    // Casting:
    public Skill CastingSkill { get; private set; }
    public bool IsCasting { get; private set; } = false;
    public double CastingStartTime { get; private set; }
    public double CastingTime { get; private set; }
    // Channeling :
    public Skill ChannelingSkill { get; private set; }
    public bool IsChanneling { get; private set; } = false;
    public double ChannelingStartTime { get; private set; }
    public double ChannelingTime { get; private set; }
    // GCD Skills :
    public double GCDStartTime { get; private set; } = 0;
    

    public EffectStack.EffectStack Stack { get; private set; }
    public Skill LastSkill { get; set; }
    
    // Todo: remove these : 
    bool hasBeenInit = false;
    bool hasBeenInitLocal = false;

    #region Godot 
    public override void _Ready()
    {
        //skillContainers = GetNode("%SkillContainers");
        Player = GetParent<PlayerEntity>();
        Stack = new EffectStack.EffectStack(this);
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
                if(IsCasting) TryInterruptCast();
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
            
            TryInterruptChanneling();
            
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

            var result = container.TriggerSkill(index);
            if(result.SUCCESS)
            {
                GD.Print("We triggered and it was success.");
                Rpc(nameof(SyncGCD), GameManager.Instance.GameClock);
                GCDStartTime = GameManager.Instance.GameClock;
                return result;
            }
            GD.Print("We triggered, but it was not success");
            return result;
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
        GD.Print("Skill result after casting:" + result.result);
        CastingSkill = null;
        IsCasting = false;
        Rpc(nameof(SyncCasting), IsCasting, CastingStartTime, 0);
    }

    public void FinishChanneling(SkillResult result)
    {
        GD.Print("Skill result after channeling:" + result.result);
        ChannelingSkill = null;
        IsChanneling = false;
        Rpc(nameof(SyncChanneling), IsChanneling, ChannelingStartTime, 0);
    }

    public void TryInterruptCast()
    {
        if (!IsCasting) return;
        
        if(CastingSkill.TimerType == MD.SkillTimerType.GCD)
        { 
            GCDStartTime -= BaseGcd;
            Rpc(nameof(SyncGCD), GCDStartTime);
        }
        CastingSkill.InterruptCast();
        CastingSkill = null;
        IsCasting = false;
        Rpc(nameof(SyncCasting), IsCasting, CastingStartTime, 0);
    }

    public void TryInterruptChanneling()
    {
        if (!IsChanneling) return;
        ChannelingSkill.InterruptChannel();
        ChannelingSkill = null;
        IsChanneling = false;
        Rpc(nameof(SyncChanneling), IsChanneling, ChannelingStartTime, 0);
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
        if(IsCasting)
        {
            result.result = MD.ActionResult.IS_CASTING;
            return result;
        }
        if(IsArsenalOnCD() && skill.TimerType == MD.SkillTimerType.GCD)
        {
            result.result = MD.ActionResult.ON_COOLDOWN;
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
        float[] weights = new float[4];
        
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
                        weights[3] += 1f;
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
        float[] totals = new float[4];
        foreach(float value in list)
        {
            totals[list.IndexOf(value)] = Mathf.Remap(value, 0, 12, 0.1f, 0.9f); 
        }
        return totals;
    }
    #endregion
}