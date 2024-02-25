using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Daikon.Game;

public partial class GameManager : Node
{

    public static GameManager Instance;    

    private bool _isConnected = false;
    private ulong _serverTick = 0;
    private ulong _clock = 0;
    private ulong _latency = 0;
    private ulong _deltaLatency = 0;
    private double _decimalCollector = 0; 
    private bool _shitCheck = false;

    //Latency Stuff
    private List<double> _latencyList = new();

    [Signal]    
    public delegate void ConnectionStartedEventHandler();

    public override void _Ready()
    {  
        if(Instance != null)
        {
            Free();
            return;
        }
        Instance = this;        
        _clock = GetSystemTimeMsec();
    }

    public ulong ServerTick
    {
        get { return _clock;}    
    }

    public void StartGame(bool isServer)
    {
        if(isServer)
        {
            _isConnected = true;
        }
        else 
        {
            SyncServerTime();
        }
    }

    public void StopGame()
    {
        _isConnected = false;
    }

    public ulong GetServerTime()
    {
        return _clock;
    }
    public ulong GetLatency()
    {
        return _latency;
    }

    private ulong GetSystemTimeMsec()
    {
        return (ulong)(Time.GetUnixTimeFromSystem() * 1000); 
    }

    private void SyncServerTime()
    {
        RpcId(1, nameof(FetchServerTime), GetSystemTimeMsec());
        Timer latencyTicker = new Timer
        {
            WaitTime = 0.5,
            Autostart = true
        };
        latencyTicker.Timeout += () => {
            RpcId(1, nameof(DetermineLatency), GetSystemTimeMsec()); 
        };
        AddChild(latencyTicker);
    }        

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void FetchServerTime(ulong clientTime)
    {
        if(!Multiplayer.IsServer())
            return;
        var peer = Multiplayer.GetRemoteSenderId();
        RpcId(peer, nameof(ReturnServerTime), GetSystemTimeMsec(), clientTime);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode =  MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ReturnServerTime(ulong serverTime, ulong clientTime)
    {
        EmitSignal(SignalName.ConnectionStarted);
        _isConnected = true;
        var latency = (GetSystemTimeMsec() - clientTime) / 2;
        _clock = (serverTime + latency);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void DetermineLatency(ulong clientTime)
    {
        if(!Multiplayer.IsServer())
            return;
         var peer = Multiplayer.GetRemoteSenderId();
         RpcId(peer, nameof(ReturnLatency), clientTime);
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode =  MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ReturnLatency(ulong clientTime)
    {
        GD.Print("Updating Latency..");
        _latencyList.Add((GetSystemTimeMsec() - clientTime) / 2);
        if(_latencyList.Count == 9)
        {
            _latencyList.Sort();
            var middle = _latencyList[4];
            GD.Print("Latency List:");
            _latencyList.ForEach(x => { GD.Print(x); });
            var noExtremes = _latencyList.Where(x => x <= (middle * 2) && x <= 20);
            var average = _latencyList.Average();
            _deltaLatency = (ulong)(average - _latency);
            _latency = (ulong)average;
            GD.Print("Latency: " + _latency );
            _latencyList.Clear();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if(_isConnected)
        {
            if(Multiplayer.IsServer())
            {
                _clock = GetSystemTimeMsec();
            }
            else
            {
                //GD.Print("yes very cool..");
                _clock += (ulong)(delta*1000) + _deltaLatency;
                _deltaLatency = 0;
                _decimalCollector += (delta * 1000) - (int)(delta * 1000);
                if(_decimalCollector >= 1)
                {
                    _clock += 1;
                    _decimalCollector -= 1;
                }
            }
        }
    }
}
