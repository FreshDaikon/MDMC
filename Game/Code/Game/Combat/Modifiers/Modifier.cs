using System.Collections.Generic;
using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem;
using Mdmc.Code.Game.Combat.Modifiers.SkillTriggers;
using Mdmc.Code.Game.Data;
using Mdmc.Code.Game.Entity.Components;

namespace Mdmc.Code.Game.Combat.Modifiers;

public partial class Modifier : Node
{
    public enum ModTags
    {
        Hot,
        Dot,
        Shield,
        Regen,
        Buff,
        Debuff,
        MoveSpeed,
        GCDSpeed,
        DamageDone,
        DamageTaken,
        Mitigation,
        HealPower,
    }

    public enum ModType
    {
        RuleMod,
        NormalMod,
    }

    // Runtime Variables:
    public required ModType Type { get; init; } 
    public required bool IsPermanent { get; init; }
    public required float Duration { get; init; }
    public required bool IsTicked { get; init; }
    public float TickRate { get; init; }
    public bool CanStack { get; init; }
    public int MaxStacks { get; init; }
    public double ModifierValue = 0; // This is very specific per mod!
    public double RemainingValue;
    public List<ModTags> Tags = new();

    public ModSkillTriggerData SkillTriggerData { get; init;}
    public int Charges = 1;

    
    // Entity to which this mod is attached:
    public EntityStatus Status {get; private set;}
    public Entity.Entity Affected { get; private set;}
    public Entity.Entity Applier { get; private set;}
    public ModifierData Data { get; init;}

    // Synced Properties:
    public int Stacks = 1;
    private double startTime;    
    public double TimeRemaining = -1f;

    //Time Keeping continued:
    private int lastLapse = 0;
    private int ticks = 0;
    private ModSkillTrigger _skillTrigger;

    
    public override void _Ready()
    {
        if(SkillTriggerData != null && Type == ModType.RuleMod)
        {
            _skillTrigger = new ModSkillTrigger()
            {
                Effect = SkillTriggerData.Effect,
                Type = SkillTriggerData.Type,
                TypeToCheck = SkillTriggerData.TypeToCheck,

            };

        }
        if(!IsPermanent)
        {
            if(Multiplayer.IsServer())
            {
                startTime = GameManager.Instance.GameClock;       
                Stacks = 1;     
                Rpc(nameof(SyncStartTime), startTime);
            }
        }          
    }

    // Please call thiS!
    public void InitData(Entity.Entity affected, Entity.Entity applier)
    {        
        Status = affected.Status;
        Affected = affected;
        Applier = applier;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncStartTime(double time)
    {
        startTime = time;
    }
    public override void _PhysicsProcess(double delta)
    {
        if(Multiplayer.IsServer())
        {   
            if(IsPermanent)
                return;
            double lapsed = GameManager.Instance.GameClock - startTime;       
            TimeRemaining = Duration - lapsed;     
            if(IsTicked)
            {
                double scaled = lapsed * TickRate;
                if((int)scaled > lastLapse)
                {
                    ticks += 1;
                    lastLapse = (int)scaled;
                    Tick();                    
                }
            }            
            if(lapsed > Duration)
            {
                GD.Print(" Total Ticks : " + ticks );
                Affected.Modifiers.RemoveModifier(Data.Id);
            }
        }        
    }
    
    public double GetTimeRemaining()
    {
        if(IsPermanent)
        {
            return 1f;
        }
        else
        {
            var lapsed = GameManager.Instance.GameClock - startTime;
            var remaining = Mathf.Clamp(Duration - lapsed, 0, Duration);
            return remaining / Duration;
        }
    }

    public void HandleSkillTrigger(Skill skill)
    {
        if(Type != ModType.RuleMod) return;
        if(_skillTrigger == null) return;

        if(_skillTrigger.CheckSkill(skill))
        {
            var effect = _skillTrigger.GetModifierEffect();
            skill.ApplyModifierEffect(effect);
            Charges -= 1;
            if(Charges <= 0)
            {
                Affected.Modifiers.RemoveModifier(Data.Id);
            }
        }
    }

    public virtual void Tick()
    {
        
    }
}