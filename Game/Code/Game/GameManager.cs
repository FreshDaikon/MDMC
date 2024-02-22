using Godot;

namespace Daikon.Game;

public partial class GameManager : Node
{

    public static GameManager Instance;    
    private ulong _serverTick = 0;

    private bool _firstTick = true;

    [Signal]    
    public delegate void ConnectionStartedEventHandler();

    public ulong ServerTick { 
        get {
            return _serverTick;
        }     
    }

    public override void _Ready()
    {  
        if(Instance != null)
        {
            Free();
            return;
        }
        Instance = this;
    }
    public override void _PhysicsProcess(double delta)
    {
        if(Multiplayer.IsServer())
        {   
            _serverTick = Time.GetTicksMsec();
            Rpc(nameof(UpdateTick), _serverTick);            
        }
    }
    // RPC Calls :
    // Sync
    // TODO : add lag ant ping.
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void UpdateTick(ulong time)
    {
        if(_firstTick)
        {
            EmitSignal(SignalName.ConnectionStarted);
            _firstTick = false;
        }
       _serverTick = time;
    }
}
