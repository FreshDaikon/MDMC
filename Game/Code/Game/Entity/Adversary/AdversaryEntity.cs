using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Daikon.Game;

public partial class AdversaryEntity : Entity
{
    [Export]
    private Node3D StartPosition;
    [Export]
    private float AggroRange = 5f;

    private TimelineManager manager;
    private AdversaryMover mover;
    private AdversaryStatus adversaryStatus;

    public TimelineManager Manager { get { return manager; }}
    public AdversaryMover Mover { get { return mover; } }
    private Dictionary<Entity, float> damageTable;
    private Dictionary<Entity, float> threatTable;

    public override void _Ready()
    {
        base._Ready();

        manager = GetNode<TimelineManager>("%TimeLineManager");
        mover = GetNode<AdversaryMover>("%Mover");
        damageTable = new Dictionary<Entity, float>();
        threatTable = new Dictionary<Entity, float>();

        adversaryStatus = (AdversaryStatus)Status;

        adversaryStatus.DamageTaken += OnDamageTaken;
        adversaryStatus.ThreatInflicted += OnThreatTaken;
        adversaryStatus.HealTaken += (heal, entity) => {};
        adversaryStatus.KnockedOut += () => {};
    }


    public override void _PhysicsProcess(double delta)
    {
        if(!Multiplayer.IsServer()) 
            return;
        base._PhysicsProcess(delta);
        if(!manager.IsEngaged)
        {
            CheckAggro();
        }
    }    

    private void CheckAggro()
    {
        var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
        if(players != null)
        {
            foreach(var player in players)
            {
                var distance = (player.Controller.Position - Controller.Position).Length();
                if(distance <= AggroRange)
                {
                    manager.Engage();
                    OnThreatTaken(500, player);
                } 
            }
        }
    }

    public void Defeat()
    {
        //Implement victory once all adversaries are dead.
    }

    public Entity GetThreatEntity(int position)
    {
         if(threatTable.Count < position+1)
            return null; 
        if(threatTable.Count > 0)
        {
            var list = threatTable.ToList();            
            list.Sort((x, y) => y.Value.CompareTo(x.Value));
            return list[position].Key;
        }
        return null;
    }

    public float GetThreatValue(int position)
    {
        if(threatTable.Count < position+1)
            return 0f;        
        if(threatTable.Count > 0)
        {
            var list = threatTable.ToList();            
            list.Sort((x, y) => y.Value.CompareTo(x.Value));
            return list[position].Value;
        }
        return 0f;
    }

    private void OnDamageTaken(float damage, Entity entity)
    {
        if(!manager.IsEngaged)
        {
            manager.Engage();
        }
        if(damageTable.ContainsKey(entity))
        {
            damageTable[entity]  += damage;
        }
        else
        {
            damageTable.Add(entity, damage);
        }
    }

    private void OnThreatTaken(float threat, Entity entity)
    {
        if(!manager.IsEngaged)
        {
            manager.Engage();
        }
        if(threatTable.ContainsKey(entity))
        {
            threatTable[entity] += threat;
        }
        else
        {
            threatTable.Add(entity, threat);
        }
    }

    public void Reset()
    {
        Status.Reset( );
        threatTable.Clear();
        damageTable.Clear();
        Controller.Teleport(StartPosition.Position);
    }
}