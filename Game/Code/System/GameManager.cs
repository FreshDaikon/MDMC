using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public partial class GameManager : Node
{
    // This Bad Boy
    public static GameManager Instance;
    public ulong ServerTick;

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
            ServerTick = Time.GetTicksMsec();
            Rpc(nameof(UpdateTick), ServerTick);
            
        }
    }
    // RPC Calls :
    // Sync
    // TODO : add lag ant ping.
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void UpdateTick(ulong time)
    {
       ServerTick = time;
    }
}
