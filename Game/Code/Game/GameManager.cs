using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Daikon.Game;

public partial class GameManager : Node
{
    public static GameManager Instance;    
    private bool _isConnected = false;
    private double _serverTick = 0;
    private double _clock = 0;
    private double _latency = 0;
    private double _deltaLatency = 0;
    private double _decimalCollector = 0; 
    //Latency Stuff
    private List<double> _latencyList = new();

    [Signal]    
    public delegate void ConnectionStartedEventHandler();

    public override void _Ready()
    {  
        if(Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;        
        _clock = GetSystemTime();
    }

    public override void _ExitTree()
    {
		if(Instance == this )
		{
			Instance = null;
		}
        base._ExitTree();
    }

    public double GameClock
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

    private double GetSystemTime()
    {
        return Time.GetUnixTimeFromSystem(); 
    }

    public double GetLatency()
    {
        return _latency;
    }

    public bool IsGameRunning()
    {
        return _isConnected;
    }

    private void SyncServerTime()
    {
        RpcId(1, nameof(FetchServerTime), GetSystemTime());
    }        

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 4)]
    private void FetchServerTime(double clientTime)
    {
        if(!Multiplayer.IsServer())
            return;
        var peer = Multiplayer.GetRemoteSenderId();
        RpcId(peer, nameof(ReturnServerTime), GetSystemTime(), clientTime);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode =  MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 4)]
    private void ReturnServerTime(double serverTime, double clientTime)
    {
        EmitSignal(SignalName.ConnectionStarted);
        _isConnected = true;
        var latency = (GetSystemTime() - clientTime) / 2;
        _clock = serverTime + latency;
        _latency = latency;
        _lastLatencyCheck = GetSystemTime();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered, TransferChannel = 3)]
    private void DetermineLatency(double clientTime)
    {
        if(!Multiplayer.IsServer())
            return;
        var peer = Multiplayer.GetRemoteSenderId();
        RpcId(peer, nameof(ReturnLatency), clientTime);
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode =  MultiplayerPeer.TransferModeEnum.UnreliableOrdered, TransferChannel = 3)]
    private void ReturnLatency(double clientTime)
    {
        _latencyList.Add((GetSystemTime() - clientTime) / 2);
        if(_latencyList.Count == 9)
        {
            _latencyList.Sort();
            var middle = _latencyList[3];
            var average = _latencyList.Average();
            _deltaLatency = average - _latency;
            _latency = average;
            _latencyList.Clear();
        }
    }

    private double _lastLatencyCheck = 0;

    public override void _PhysicsProcess(double delta)
    {
        if(_isConnected)
        {
            if(Multiplayer.IsServer())
            {
                _clock = GetSystemTime();
            }
            else
            {
                if(GetSystemTime() - _lastLatencyCheck > 0.5)
                {
                    _lastLatencyCheck = GetSystemTime();
                    RpcId(1, nameof(DetermineLatency), GetSystemTime()); 
                }
                _clock += delta + _deltaLatency;
                _deltaLatency = 0;
            }
        }
    }
}
