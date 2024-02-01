using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public partial class GameManager : Node
{
    // This Bad Boy
    public static GameManager Instance;
    //Code Containers:
    //Scene Containers:
    private Node3D EntityContainer;
    private Node3D RealizationPool;

    [Export]
    public ulong ServerTick;

    [Export]
    public bool isDebug = false;
    //Point this to the correct Scene Paths:
    [Export]
    private string playerEntityPath;

    //
    // In _Ready we init our variables, and make sure instance is singleton.
    //
    public override void _Ready()
    {  
        if(Instance != null)
        {
            Free();
            return;
        }
        Instance = this;
        EntityContainer = GetNode<Node3D>("%Entities");
        RealizationPool = GetNode<Node3D>("%RealizationPool");
        if(Multiplayer.IsServer())
        {
            isDebug = true;
        }
    }
    public Node3D GetRealizationPool()
    {
        return RealizationPool;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(Multiplayer.IsServer())
        {   
            ServerTick = Time.GetTicksMsec();
        }
        base._PhysicsProcess(delta);
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
