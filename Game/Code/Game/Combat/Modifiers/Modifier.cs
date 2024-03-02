using Godot;
using Daikon.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Daikon.Game;

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
    // Runtime Variables:
    public bool IsPermanent = false;
    public float Duration = 5f;
    public bool IsTicked = false;
    public float TickRate = 1f;
    public bool CanStack = false;
    public int MaxStacks = 1;
    public double ModifierValue = 0; // This is very specific per mod!
    public double RemainingValue;
    public List<ModTags> Tags = new();

    // Synced Properties:
    public int Stacks = 1;
    private double startTime;    
    public double TimeRemaining = -1f;

    public ModifierData Data;
    //Time Keeping continued:
    private int lastLapse = 0;
    private int ticks = 0;

    // Entity to which this mod is attached:
    public EntityStatus targetStatus;
    public Entity entity;
    //Entity who applied this modifier:   
    
    public override void _Ready()
    {
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
                entity.Modifiers.RemoveModifier(Data.Id);
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
    public virtual void Tick()
    {
        
    }
}