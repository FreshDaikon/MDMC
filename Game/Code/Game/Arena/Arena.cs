

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Godot;

public partial class Arena : Node3D
{
    [Export]
    private DataID ID;
    public int Id
    {
        get { return ID.Id; }
    }

    [Export]
    public float ArenaDuration = 120f;
    private ulong StartTime;
    private Node3D EntityContainer;
    private bool arenaFailed = false;

    public override void _Ready()
    {
        StartTime = GameManager.Instance.ServerTick;
        EntityContainer = GetNode<Node3D>("%EntityContainer");
    }

    public override void _Process(double delta)
    {
        CheckArenaFailedState();
    }


    private void CheckArenaFailedState()
    {
        var lapsed = (GameManager.Instance.ServerTick - StartTime) / 60000f;
        if(lapsed >= ArenaDuration)
        {
            arenaFailed = true;
        }
        // Other fail states go here:
    }

    public bool GetArenaFailedState()
    {
        return arenaFailed;
    }

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
        MD.Log("Index : " + enemies.IndexOf(enemies.Find(p => p.Name == id.ToString())));
        return enemies.IndexOf(enemies.Find(p => p.Name == id.ToString()));        
    }
    public int GetFriendlyIndex(int id)
    {
        MD.Log("Id to find: " + id);
        if(EntityContainer.GetChildCount() == 0)
            return -1;
        var friends = EntityContainer.GetChildren()
            .Where(child => child is Entity)
            .Cast<Entity>()
            .Where(e => e.Team == Entity.TeamType.Friend)
            .ToList();
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
        var players = EntityContainer.GetChildren()
            .Where(child => child is PlayerEntity)
            .Cast<PlayerEntity>()
            .ToList();        
        return players;
    }
    public List<Entity> GetFriendlyEntities()
    {
        var entities = EntityContainer.GetChildren()
            .Where(child => child is Entity)
            .Cast<Entity>()
            .Where(e => e.Team == Entity.TeamType.Friend)
            .ToList();        
        return entities;
    }
    public List<Entity> GetEnemyEntities()
    {
        var entities = EntityContainer.GetChildren()
            .Where(child => child is Entity)
            .Cast<Entity>()
            .Where(e => e.Team == Entity.TeamType.Foe)
            .ToList();        
        return entities;
    }
}