using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class ArenaManager : Node
{  

    public static ArenaManager Instance;   
    private Node3D _arenaContainer;
    private Arena _currentArena;

    //Events for users who rely on this singleton:
    [Signal]
    public delegate void ArenaVictoryEventHandler();
    [Signal]
    public delegate void ArenaDefeatEventHandler();

    public override void _Ready()
    {
        if(Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        _arenaContainer = GetNode<Node3D>("%ArenaContainer");
        base._Ready();
    }

    public Arena GetCurrentArena()
    {
        return _currentArena;
    }

    public Node3D GetRealizationPool()
    {
        if(_currentArena != null)
        {
            return _currentArena.RealizationPool;
        }
        return null;
    }

    public bool HasArena()
    {
        return _currentArena != null;
    }

    public bool LoadArena(int id)
    {
        GD.Print("ArenaManager id to load is : " + id);
        var arena = DataManager.Instance.GetArenaInstance(id);
        if(arena != null)
        {
            UnloadArena();
            _arenaContainer.AddChild(arena);
            _currentArena = arena;
            _currentArena.Victory += () => { Rpc(nameof(SyncEventVictory)); };
            _currentArena.Defeat += () => { Rpc(nameof(SyncEventDefeat)); };
            return true;
        }
        else
        {
            return false;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncEventDefeat()
    {
        EmitSignal( SignalName.ArenaDefeat);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncEventVictory()
    {
        EmitSignal( SignalName.ArenaVictory);
    }

    public void UnloadArena()
    {
        if(_currentArena != null)
        {
            _arenaContainer.RemoveChild(_currentArena);
            _currentArena.QueueFree();
        }
        else
        {
            GD.Print("No Arena to unload..");
        }
    }
}