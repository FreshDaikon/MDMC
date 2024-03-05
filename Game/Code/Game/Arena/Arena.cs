using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Daikon.Game;

public partial class Arena : Node3D
{
    public ArenaData Data;
    [Export] private double _arenaDuration = 7200;
    [Export] private Node3D[] _playerStartPositions;
    //containers:
    private Node3D _entityContainer;
    //How Long has the arena been going in minutes:
    private double _startTime;
    private double _lapsed;
    private Timer _defeatTimer; 

    public ArenaState CurrentState { get; private set; } = ArenaState.Paused;

    [Signal]    
    public delegate void VictoryEventHandler();
    [Signal]
    public delegate void DefeatEventHandler();

    public override void _Ready()
    {
        _entityContainer = GetNode<Node3D>("%EntityContainer");
        GameManager.Instance.GameStarted += StartArena;
    }

    public override void _Process(double delta)
    {
        if(CurrentState == ArenaState.Playing)
        {
            if(Multiplayer.IsServer())
            {
                CheckForVictory();
                CheckForWipe();
            }
            _lapsed = GameManager.Instance.GameClock - _startTime;
        }
    }

    private void StartArena()
    {
        _startTime = GameManager.Instance.GameClock;            
        CurrentState = ArenaState.Playing;
        if (!Multiplayer.IsServer()) return;
        _defeatTimer = new Timer(){
            Autostart = false,
            WaitTime = 5,
            OneShot = true
        };
        var enemies = GetEnemyEntities();
        if(enemies != null)
        {
            enemies.ForEach(e => { e.Engaged += OnAdversaryEngaged; });
        }
        _defeatTimer.Timeout += ResetArena;
        AddChild(_defeatTimer);
    }

    public double GetTimeLeft() => Mathf.Clamp(_arenaDuration - _lapsed, 0, _arenaDuration);

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncStartTime(double time)
    {
        _startTime = time;
    }

    private void OnAdversaryEngaged()
    {
        var current = CombatManager.Instance.CurrentState;
        if(current is CombatManager.CombatState.None or CombatManager.CombatState.Stopped)
        {            
            CombatManager.Instance.StartCombat();            
        }
        var enemies = GetEnemyEntities();
        var players = GetPlayers();
        enemies?.ForEach( a => {
                if(a.CurrentState == AdversaryState.Idle)
                {
                    a.Engage(players?[0]);
                }
        });
    }

    private void CheckForWipe()
    {
        var players = GetPlayers();
        
        if (players == null) return;
        
        var knocked = players.Where(p => p.Status.CurrentState == EntityStatus.StatusState.KnockedOut).ToList();
        
        if (knocked.Count != players.Count) return;
        //:
        GD.Print("All Players are knocked! Init Reset!");                
        CurrentState = ArenaState.Defeat;
        CombatManager.Instance.ResetCombat();
        EmitSignal(SignalName.Defeat);
        _defeatTimer.Start();
    }

    private void CheckForVictory()
    {
        var enemies = GetEnemyEntities();
        if (enemies == null) return;
        var defeated = enemies.Where(e => e.CurrentState == AdversaryState.Dead).ToList();
        if (defeated.Count != enemies.Count) return;
        foreach(var e in enemies) e.QueueFree();                
        EmitSignal(SignalName.Victory);
        CurrentState = ArenaState.Victory;
    }

    private void ResetArena()
    {        
        var players = GetPlayers();
        if(players != null)
        {
            foreach(var p in players)
            {
                p.Controller.Teleport(_playerStartPositions[Mathf.Wrap(players.IndexOf(p), 0, _playerStartPositions.Length)].Position);
                p.Status.Reset();
            }
        }
        var adversaries = GetEnemyEntities();
        if(adversaries != null)
        {
            foreach(var a in adversaries)
            {
                a.Reset();
            }
        }
        CurrentState = ArenaState.Playing;
    }

    public void AddPlayerEntity(PlayerEntity player)
    {
        if (!Multiplayer.IsServer()) return;
        
        var players = GetPlayers();
        var count = players == null ? 0 : player.GetChildCount();
        _entityContainer.AddChild(player);
        player.Controller.Teleport(_playerStartPositions[Mathf.Wrap(count, 0, _playerStartPositions.Length)].GlobalPosition);
    }

    public void RemovePlayerEntity(int id)
    {
        if (!Multiplayer.IsServer()) return;
        
        if(_entityContainer.GetChildCount() == 0)
            return;
        var player = _entityContainer.GetChildren().Where(p => p is PlayerEntity).Cast<PlayerEntity>().ToList()
            .Find(e => e.Name == id.ToString());        
        _entityContainer.RemoveChild(player);
    }

    public Entity GetEntity(int id) => _entityContainer.GetChildCount() == 0
        ? null
        : _entityContainer.GetChildren().Where(x => x is Entity).Cast<Entity>().ToList()
            .Find(e => e.Name == id.ToString());

    public int GetEntityIndex(int id)
    {
        if(_entityContainer.GetChildCount() == 0)
            return -1;
        return _entityContainer.GetNode(id.ToString()).GetIndex();
    }
    public int GetEnemyIndex(int id)
    {
        if(_entityContainer.GetChildCount() == 0)
            return -1;
        var enemies = _entityContainer.GetChildren()
            .Where(child => child is AdversaryEntity)
            .Cast<AdversaryEntity>()
            .ToList();
        if(enemies.Count == 0)
            return -1;
        return enemies.IndexOf(enemies.Find(p => p.Name == id.ToString()));
    }

    public int GetFriendlyIndex(int id)
    {
        if(_entityContainer.GetChildCount() == 0)
            return -1;
        var friends = _entityContainer.GetChildren()
            .Where(child => child is Entity)
            .Cast<Entity>()
            .Where(e => e.Team == Entity.TeamType.Player)
            .ToList();
        if(friends.Count == 0)
            return -1;
        return friends.IndexOf(friends.Find(p => p.Name == id.ToString())); 
    }
    public List<Entity> GetEntities()
    {
        if(_entityContainer.GetChildCount() == 0)
            return null;
        var entities = _entityContainer.GetChildren()
            .Where(child => child is Entity)
            .Cast<Entity>()
            .ToList();        
        return entities;
    }
    public List<PlayerEntity> GetPlayers()
    {
        if(_entityContainer.GetChildren().Count == 0)
            return null;
        var players = _entityContainer.GetChildren()
            .Where(child => child is PlayerEntity)
            .Cast<PlayerEntity>()
            .ToList();     
        return players.Count == 0 ? null : players;
    }
    public List<Entity> GetFriendlyEntities()
    {
        if(_entityContainer.GetChildren().Count == 0)
            return null;
        var entities = _entityContainer.GetChildren()
            .Where(child => child is Entity)
            .Cast<Entity>()
            .Where(e => e.Team == Entity.TeamType.Player)
            .ToList();        
        return entities.Count == 0 ? null : entities;
    }
    public List<AdversaryEntity> GetEnemyEntities()
    {
        if(_entityContainer.GetChildren().Count == 0)
            return null;
        var entities = _entityContainer.GetChildren()
            .Where(child => child is AdversaryEntity)
            .Cast<AdversaryEntity>()
            .ToList();        
        return entities.Count == 0 ? null : entities;
    }
}

public enum ArenaState
{
    Playing,
    Paused,
    Victory,
    Defeat,
}