using System.Collections.Generic;
using System.Linq;
using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class Arena : Node3D
{
    [Export]
    private DataID ID;
    public int Id
    {
        get { return ID.Id; }
    }

    [Export]
    public string ArenaName = "Some Cool Arena.";
    [Export]
    public float ArenaDuration = 120f;
    //containers:
    private Node3D EntityContainer;
    public Node3D RealizationPool;
    //How Long has the arena been going in minutes:
    private ulong _startTime;
    private float _lapsed;

    public override void _Ready()
    {
        if(Multiplayer.IsServer())
        {
            _startTime = GameManager.Instance.ServerTick;
            Rpc(nameof(SyncStartTime), _startTime);
        }
        EntityContainer = GetNode<Node3D>("%EntityContainer");
        RealizationPool = GetNode<Node3D>("%RealizationPool");
    }

    public override void _Process(double delta)
    {
        _lapsed = (GameManager.Instance.ServerTick - _startTime) / 60000f;
    }

    public float GetTimeLeft()
    {
        return Mathf.Clamp(ArenaDuration - _lapsed, 0, ArenaDuration);
    }


    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]  
    public void SyncStartTime(ulong time)
    {
        _startTime = time;
    }
    //
    // Add and Remove Player Entities:
    //
    public void AddPlayerEntity(PlayerEntity player)
    {
        if(Multiplayer.IsServer())
        {
            EntityContainer.AddChild(player);
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

    //
    // Fetch Various Entities:
    //
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
        MD.Log("Id to find: " + id);
        if(EntityContainer.GetChildCount() == 0)
            return -1;
        var enemies = EntityContainer.GetChildren()
            .Where(child => child is Entity)
            .Cast<Entity>()
            .Where(e => e.Team == Entity.TeamType.Foe)
            .ToList();
        if(enemies.Count == 0)
            return -1;
        MD.Log("Index : " + enemies.IndexOf(enemies.Find(p => p.Name == id.ToString())));
        return enemies.IndexOf(enemies.Find(p => p.Name == id.ToString()));        
    }
    public int GetFriendlyIndex(int id)
    {
        if(EntityContainer.GetChildCount() == 0)
            return -1;
        var friends = EntityContainer.GetChildren()
            .Where(child => child is Entity)
            .Cast<Entity>()
            .Where(e => e.Team == Entity.TeamType.Friend)
            .ToList();
        if(friends.Count == 0)
            return -1;
        MD.Log("Index : " + friends.IndexOf(friends.Find(p => p.Name == id.ToString())));
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
            .Where(e => e.Team == Entity.TeamType.Friend)
            .ToList();        
        if(entities.Count == 0)
            return null;
        return entities;
    }
    public List<Entity> GetEnemyEntities()
    {
        if(EntityContainer.GetChildren().Count == 0)
            return null;
        var entities = EntityContainer.GetChildren()
            .Where(child => child is Entity)
            .Cast<Entity>()
            .Where(e => e.Team == Entity.TeamType.Foe)
            .ToList();        
        if(entities.Count == 0)
            return null;
        return entities;
    }
}