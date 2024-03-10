using Godot;

namespace Mdmc.Code.Game.Arena;

public partial class ArenaManager : Node
{  
    // Public Access:
    public static ArenaManager Instance;   
    
    // Internals:
    private Node3D _arenaContainer;
    private ArenaInstance _currentArenaInstance;

    // Signals:
    [Signal] public delegate void ArenaVictoryEventHandler();
    [Signal] public delegate void ArenaDefeatEventHandler();

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

    public Mdmc.Code.Game.Arena.ArenaInstance GetCurrentArena()
    {
        return _currentArenaInstance;
    }

    public bool HasArena()
    {
        return _currentArenaInstance != null;
    }

    public bool LoadArena(int id)
    {
        GD.Print("ArenaManager id to load is : " + id);
        var arena = DataManager.Instance.GetArenaInstance(id);
        if(arena != null)
        {
            UnloadArena();
            _arenaContainer.AddChild(arena);
            _currentArenaInstance = arena;
            _currentArenaInstance.Victory += () => { Rpc(nameof(SyncEventVictory)); };
            _currentArenaInstance.Defeat += () => { Rpc(nameof(SyncEventDefeat)); };
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
        EmitSignal(SignalName.ArenaDefeat);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncEventVictory()
    {
        EmitSignal(SignalName.ArenaVictory);
    }

    public void UnloadArena()
    {
        if(_currentArenaInstance != null)
        {
            _arenaContainer.RemoveChild(_currentArenaInstance);
            _currentArenaInstance.QueueFree();
        }
        else
        {
            GD.Print("No Arena to unload..");
        }
    }
}