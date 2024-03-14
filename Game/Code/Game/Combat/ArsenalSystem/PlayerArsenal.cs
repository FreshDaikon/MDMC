using System.Linq;
using Godot;
using Mdmc.Code.Game.Entity.Player;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Combat.ArsenalSystem;

public partial class PlayerArsenal: Node
{
    [Export] public float BaseGcd { get; private set; } = 1.5f;   
    // Player :
    public PlayerEntity Player { get; private set; }
    // Casting:
    public SkillHandler CastingSkill { get; private set; }
    public CastController CastingController { get; private set; }
    public bool IsCasting { get; private set; } = false;
    public double CastingStartTime { get; private set; }
    public double CastingTime { get; private set; }
    // Channeling :
    public SkillHandler ChannelingSkill { get; private set; }
    public ChannelController ChannelingController { get; private set; }
    public bool IsChanneling { get; private set; } = false;
    public double ChannelingStartTime { get; private set; }
    public double ChannelingTime { get; private set; }
    // GCD Skills :
    public double GCDStartTime { get; private set; } = 0;
    public SkillHandler LastSkillTriggered { get; private set; }
    
    // Todo: remove these : 
    bool hasBeenInit = false;
    bool hasBeenInitLocal = false;

    #region Godot 
    public override void _Ready()
    {
        //skillContainers = GetNode("%SkillContainers");
        Player = GetParent<PlayerEntity>();
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
        var slots = new MD.ContainerSlot[]{
            MD.ContainerSlot.Main, 
            MD.ContainerSlot.Right,
            MD.ContainerSlot.Left};
        foreach(var slot in slots)
        {
            var firstContainer = containers[0];
            GD.Print("Set Container : " + firstContainer.Name + " To Container: " + slot);
            SetSkillContainer(firstContainer.Id, slot);
            Rpc(nameof(SyncSkillContainer), firstContainer.Id, (int)slot);
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
    }    
    
    public SkillHandler GetSkill(MD.ContainerSlot containerSlot, int slot)
    {
        var container = GetSkillContainer(containerSlot);
        if(container != null)
        {
            return container.GetSkill(slot);
        }
        return null;
    }

    public void TrySetContainer(int id, int slot)
    {
        RpcId(1, nameof(RequestContainerChange), id, slot);
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
            
            var skill = container.GetSkill(index);

            if(skill.GetTypeInfo() == SkillHandler.SkillType.OGCD)
            {
                GD.Print("Try Trigger OGCD Skill:");
                var ogcdResult = container.TriggerSkill(index);
                if(!ogcdResult.SUCCESS) return ogcdResult;
                LastSkillTriggered = skill;
                UpdateContainerStates();
                
                return ogcdResult;
            }            
            
            var lapsed = GameManager.Instance.GameClock - GCDStartTime;
            //TODO : Add gcd modifiers from skill containers here!
            var modGCD = BaseGcd; 
            if(lapsed < modGCD)
            {
                return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.ON_COOLDOWN };
            }

            var result = container.TriggerSkill(index);
            if(!result.SUCCESS) return result;
            if(result.result == MD.ActionResult.START_CASTING)
            {
                LastSkillTriggered = skill;
                StartCasting(skill, result.extraData.controller);
                return result;
            }
            if(result.result == MD.ActionResult.START_CHANNELING)
            {
                LastSkillTriggered = skill;
                StartCasting(skill, result.extraData.controller);
                return result;
            }          
            GD.Print("We triggered and it was success and it was not cast or channel!");
            LastSkillTriggered = skill;
            UpdateContainerStates();
            Rpc(nameof(SyncGCD), GameManager.Instance.GameClock);
            GCDStartTime = GameManager.Instance.GameClock;
            return result;
        }
        return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.ERROR };
    }

    private void UpdateContainerStates()
    {
        if(GetChildCount() <= 0 ) return;
        GetChildren().Where(c => c is SkillContainer).Cast<SkillContainer>().ToList().ForEach(x => x.UpdateComponentStates());
    }

    public void StartCasting(SkillHandler skill, CastController controller)
    {
        IsCasting = true;
        if(skill.GetTypeInfo() == SkillHandler.SkillType.GCD)
        {
            GCDStartTime = GameManager.Instance.GameClock;
            Rpc(nameof(SyncGCD), GameManager.Instance.GameClock);
        }
        CastingStartTime = GameManager.Instance.GameClock;
        CastingTime = controller.CurrentCastTime;
        Rpc(nameof(SyncCasting), IsCasting, CastingStartTime, CastingTime);
        CastingController = controller;
        CastingController.FinishedCasting += FinishCasting;
        CastingSkill = skill;
    }

    public void StartChanneling(SkillHandler skill, ChannelController controller)
    {
        IsChanneling = true;
        if(skill.GetTypeInfo() == SkillHandler.SkillType.GCD)
        {
            GCDStartTime = GameManager.Instance.GameClock;
            Rpc(nameof(SyncGCD), GameManager.Instance.GameClock);
        }
        ChannelingStartTime = GameManager.Instance.GameClock;
        ChannelingTime = controller.CurrentChannelTime;
        ChannelingSkill = skill;
        ChannelingController = controller;
        ChannelingController.FinishedChanneling += FinishChanneling;
        Rpc(nameof(SyncChanneling), IsChanneling, ChannelingStartTime, ChannelingTime);
    }

    public void FinishCasting()
    {
        CastingSkill = null;
        IsCasting = false;
        CastingController.FinishedCasting -= FinishCasting;
        CastingController = null;
        Rpc(nameof(SyncCasting), IsCasting, CastingStartTime, 0);
    }

    public void FinishChanneling()
    {
        ChannelingSkill = null;
        IsChanneling = false;
        ChannelingController.FinishedChanneling -= FinishChanneling;
        ChannelingController = null;
        Rpc(nameof(SyncChanneling), IsChanneling, ChannelingStartTime, 0);
    }

    public void TryInterruptCast()
    {
        if (!IsCasting) return;
        
        if(CastingSkill.GetTypeInfo() == SkillHandler.SkillType.GCD)
        { 
            GCDStartTime -= BaseGcd;
            Rpc(nameof(SyncGCD), GCDStartTime);
        }
        CastingController.Interrupt(true);
        CastingController.FinishedCasting -= FinishCasting;
        CastingController = null;
        CastingSkill = null;
        IsCasting = false;
        Rpc(nameof(SyncCasting), IsCasting, CastingStartTime, 0);
    }

    public void TryInterruptChanneling()
    {
        if (!IsChanneling) return;
        ChannelingController.Interrupt(true);
        ChannelingController.FinishedChanneling -= FinishChanneling;
        ChannelingController = null;
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
    
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncSkillContainer(int id, int containerSlot)
    {
        GD.Print("Adding skillContainer on client for " + containerSlot);
        SetSkillContainer(id, (MD.ContainerSlot)containerSlot);
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
            return new SkillResult() { SUCCESS = false, result= MD.ActionResult.ERROR };
        }
        var skill = container.GetSkill(slot);
        if(skill == null)
        {
            return new SkillResult() { SUCCESS = false, result= MD.ActionResult.ERROR };
        }
        if(IsCasting)
        {
            return new SkillResult() { SUCCESS = false, result= MD.ActionResult.IS_CASTING };
        }
        if(IsArsenalOnCD() && skill.GetTypeInfo() != SkillHandler.SkillType.OGCD)
        {
            return new SkillResult() { SUCCESS = false, result= MD.ActionResult.ON_COOLDOWN };
        }  
        if(skill.GetTimeInfo().IsOnCooldown)
        {            
            return new SkillResult() { SUCCESS = false, result= MD.ActionResult.ON_COOLDOWN };
        }
        var quickCheck = skill.CheckSkill();
        if(!quickCheck.SUCCESS)
        {
            return result;
        }
        return new SkillResult() { SUCCESS = true, result= MD.ActionResult.CAST };
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

    #endregion
}