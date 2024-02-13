using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public partial class GameManager : Node
{

    public static GameManager Instance;
    
    private ulong _serverTick = 0;
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
       _serverTick = time;
    }
}
