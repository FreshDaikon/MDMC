using System.Collections.Generic;
using System.Linq;
using Godot;
using Daikon.Helpers;
using System;

namespace Daikon.Game;

public partial class Arena : Node3D
{
    public ArenaObject Data;
    [Export]
    public double ArenaDuration = 7200;
    [Export]
    public Node3D[] PlayerStartPositions;
    //containers:
    private Node3D EntityContainer;
    public Node3D RealizationPool;
    //How Long has the arena been going in minutes:
    private double _startTime;
    private double _lapsed;
    private ArenaState _currentState = ArenaState.Paused;
    private Timer DefeatTimer; 

    public ArenaState CurrentState
    {
        get {return _currentState; }
    }

    public enum ArenaState
    {
        Playing,
        Paused,
        Victory,
        Defeat,
    }

    [Signal]    
    public delegate void VictoryEventHandler();
    [Signal]
    public delegate void DefeatEventHandler();

    public override void _Ready()
    {
        EntityContainer = GetNode<Node3D>("%EntityContainer");
        RealizationPool = GetNode<Node3D>("%RealizationPool");
        GameManager.Instance.GameStarted += StartArena;
    }

    public override void _Process(double delta)
    {
        if(_currentState == ArenaState.Playing)
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
        _currentState = ArenaState.Playing;
        if(Multiplayer.IsServer())
        {
            DefeatTimer = new Timer(){
                Autostart = false,
                WaitTime = 5,
                OneShot = true
            };
            var enemies = GetEnemyEntities();
            if(enemies != null)
            {
                enemies.ForEach(e => { e.Engaged += OnAdversaryEngaged; });
            }
            DefeatTimer.Timeout += ResetArena;
            AddChild(DefeatTimer);
        }
    }

    public double GetTimeLeft()
    {
        return Mathf.Clamp(ArenaDuration - _lapsed, 0, ArenaDuration);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]  
    public void SyncStartTime(double time)
    {
        _startTime = time;
    }

    private void OnAdversaryEngaged()
    {
        var current = CombatManager.Instance.CurrentState;
        if(current == CombatManager.CombatState.None || current == CombatManager.CombatState.Stopped)
        {            
            CombatManager.Instance.StartCombat();            
        }
        var enemies = GetEnemyEntities();
        var players = GetPlayers();
        enemies?.ForEach( a => {
                if(a.CurrentState == AdversaryEntity.AdversaryState.Idle)
                {
                    a.Engage(players?[0]);
                }
            });
    }

    private void CheckForWipe()
    {
        var players = GetPlayers();
        if(players != null)
        {
            var knocked = players.Where(p => p.Status.IsKnockedOut).ToList();
            if(knocked.Count == players.Count)
            {
                GD.Print("All Players are knocked! Init Reset!");                
                _currentState = ArenaState.Defeat;
                CombatManager.Instance.ResetCombat();
                EmitSignal(SignalName.Defeat);
                DefeatTimer.Start();
            }
        }
    }

    private void CheckForVictory()
    {
        var enemies = GetEnemyEntities();
        if(enemies != null)
        {
            var defeated = enemies.Where(e => e.CurrentState == AdversaryEntity.AdversaryState.Dead).ToList();
            if(defeated.Count == enemies.Count)
            {
                foreach(var e in enemies)
                {
                    e.QueueFree();
                }                
                EmitSignal(SignalName.Victory);
                _currentState = ArenaState.Victory;
            }
        }
    }

    // Should only be called on wipes.
    private void ResetArena()
    {        
        var players = GetPlayers();
        if(players != null)
        {
            foreach(var p in players)
            {
                p.Controller.Teleport(PlayerStartPositions[Mathf.Wrap(players.IndexOf(p), 0, PlayerStartPositions.Length)].Position);
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
        _currentState = ArenaState.Playing;
    }

    public void AddPlayerEntity(PlayerEntity player)
    {
        if(Multiplayer.IsServer())
        {
            GD.Print("Adding Player!");
            var players = GetPlayers();
            int count = 0;
            if(players != null)
            {
                count = players.Count;
            }
            EntityContainer.AddChild(player);
            player.Controller.Teleport(PlayerStartPositions[Mathf.Wrap(count, 0, PlayerStartPositions.Length)].GlobalPosition);
        }
    }

    public void RemovePlayerEntity(int id)
    {
        if(Multiplayer.IsServer())
        {
            if(EntityContainer.GetChildCount() == 0)
                return;
            var player = EntityContainer.GetChildren().Where(p => p is PlayerEntity).Cast<PlayerEntity>().ToList().Find(e => e.Name == id.ToString());        
            EntityContainer.RemoveChild(player);
        }
    }

    public Entity GetEntity(int id)
    {
        if(EntityContainer.GetChildCount() == 0)
            return null;
        return EntityContainer.GetChildren().Where(x => x is Entity).Cast<Entity>().ToList().Find(e => e.Name == id.ToString());
    }
    public int GetEntityIndex(int id)
    {
        if(EntityContainer.GetChildCount() == 0)
            return -1;
        return EntityContainer.GetNode(id.ToString()).GetIndex();
    }
    public int GetEnemyIndex(int id)
    {
        GD.Print("Id to find: " + id);
        if(EntityContainer.GetChildCount() == 0)
            return -1;
        var enemies = EntityContainer.GetChildren()
            .Where(child => child is AdversaryEntity)
            .Cast<AdversaryEntity>()
            .ToList();
        if(enemies.Count == 0)
            return -1;
        GD.Print("Index : " + enemies.IndexOf(enemies.Find(p => p.Name == id.ToString())));
        return enemies.IndexOf(enemies.Find(p => p.Name == id.ToString()));        
    }
    public int GetFriendlyIndex(int id)
    {
        if(EntityContainer.GetChildCount() == 0)
            return -1;
        var friends = EntityContainer.GetChildren()
            .Where(child => child is Entity)
            .Cast<Entity>()
            .Where(e => e.Team == Entity.TeamType.Player)
            .ToList();
        if(friends.Count == 0)
            return -1;
        GD.Print("Index : " + friends.IndexOf(friends.Find(p => p.Name == id.ToString())));
        return friends.IndexOf(friends.Find(p => p.Name == id.ToString())); 
    }
    public List<Entity> GetEntities()
    {
        if(EntityContainer.GetChildCount() == 0)
            return null;
        var entities = EntityContainer.GetChildren()
            .Where(child => child is Entity)
            .Cast<Entity>()
            .ToList();        
        return entities;
    }
    public List<PlayerEntity> GetPlayers()
    {
        if(EntityContainer.GetChildren().Count == 0)
            return null;
        var players = EntityContainer.GetChildren()
            .Where(child => child is PlayerEntity)
            .Cast<PlayerEntity>()
            .ToList();     
        if(players.Count == 0)
            return null;   
        return players;
    }
    public List<Entity> GetFriendlyEntities()
    {
        if(EntityContainer.GetChildren().Count == 0)
            return null;
        var entities = EntityContainer.GetChildren()
            .Where(child => child is Entity)
            .Cast<Entity>()
            .Where(e => e.Team == Entity.TeamType.Player)
            .ToList();        
        if(entities.Count == 0)
            return null;
        return entities;
    }
    public List<AdversaryEntity> GetEnemyEntities()
    {
        if(EntityContainer.GetChildren().Count == 0)
            return null;
        var entities = EntityContainer.GetChildren()
            .Where(child => child is AdversaryEntity)
            .Cast<AdversaryEntity>()
            .ToList();        
        if(entities.Count == 0)
            return null;
        return entities;
    }
}