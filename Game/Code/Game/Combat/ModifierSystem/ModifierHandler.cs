using Godot;
using Mdmc.Code.Game.Data;
using Mdmc.Code.Game.Entity.Components;

namespace Mdmc.Code.Game.Combat.ModifierSystem;

[GlobalClass]
public partial class ModifierHandler : Node
{
    // Runtime Variables:
    [Export] public bool IsPermanent { get; private set; }
    [Export] public bool IsHidden { get; private set; }
    [Export] public float Duration { get; private set; }
    [Export] public int MaxStacks { get; private set; } = 1;
    [Export] public BuffController BuffControl { get; private set; }
    [Export] public TickController Ticker { get; private set; }


    // Synced Properties:
    public int Stacks { get; private set; } = 1;
    public double StartTime { get; private set; }
    public double TimeRemaining { get; private set; } = -1f;

    // Entity to which this mod is attached:
    public Entity.Entity Affected { get; private set;}
    public Entity.Entity Applier { get; private set;}
    public ModifierData Data { get; set;}

    public bool Terminated { get; private set; } = false;

    public override void _Ready()
    {
        if(Multiplayer.IsServer())
        {
            StartTime = GameManager.Instance.GameClock;   
            Stacks = 1;     
        }
        else
        {
            RpcId(1, nameof(RequestStartTime));
        }
    }

    // Please call thiS!
    public void SetLiveData(Entity.Entity affected, Entity.Entity applier)
    {        
        Affected = affected;
        Applier = applier;
    }    

    public void AddStack()
    {
        if(Stacks == MaxStacks) return;
        // Refresh:
        StartTime = GameManager.Instance.GameClock;
        Stacks ++;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(Multiplayer.IsServer())
        {   
            if(IsPermanent) return;
            if(Terminated) return;

            double lapsed = GameManager.Instance.GameClock - StartTime;       
            TimeRemaining = Duration - lapsed;            
            if(lapsed > Duration)
            {
                Terminate();
            }
        }        
    }

    public void Terminate()
    {
        Terminated = true;
        Affected.Modifiers.RemoveModifier(Data.Id);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestStartTime()
    { 
        if(Multiplayer.IsServer())
        {
            Rpc(nameof(SyncStartTime), StartTime);
        }

    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncStartTime(double time)
    {
        StartTime = time;
    }
}