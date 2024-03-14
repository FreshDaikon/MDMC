using Godot;
using Mdmc.Code.Game;

[GlobalClass]
public partial class TimeController : Node 
{
    // Exports:
    [Export] public float BaseCooldown { get; private set; }
    // Exposed:
    public bool IsOnCooldown { get; private set; }
    public float CurrentCooldown { get; private set; }
    public double StartTime { get; private set; }
    public double Lapsed  { get; private set; }

    [Signal] public delegate void CooldownFinishedEventHandler();

    public override void _PhysicsProcess(double delta)
    {
        if(IsOnCooldown)
        {
            Lapsed = GameManager.Instance.GameClock - StartTime;            
            if(Lapsed > CurrentCooldown)
            {
                CurrentCooldown = BaseCooldown;
                IsOnCooldown = false;
                EmitSignal(SignalName.CooldownFinished);
                Rpc(nameof(SyncTimeInfo), IsOnCooldown, -1, CurrentCooldown);
            }
        }
    }

    public void StartCooldown()
    {
        if(BaseCooldown == 0) return;
        IsOnCooldown = true;
        CurrentCooldown = BaseCooldown;
        StartTime = GameManager.Instance.GameClock;
        Rpc(nameof(SyncTimeInfo), IsOnCooldown, StartTime, CurrentCooldown);
    }

    public void ResetCooldown()
    {
        IsOnCooldown = false;
        CurrentCooldown = BaseCooldown;
        Rpc(nameof(SyncTimeInfo), IsOnCooldown, -1, CurrentCooldown);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncTimeInfo(bool cooldown, double startTime, float currentCd)
    {
        IsOnCooldown = cooldown;
        StartTime = startTime;
        CurrentCooldown = currentCd;
    }
}