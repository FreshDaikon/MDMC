using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Daikon.Game;

public partial class AdversaryEntity : Entity
{
    [Export]
    private Node3D StartPosition;

    private TimelineManager manager;
    private AdversaryMover mover;
    private AdversaryStatus adversaryStatus;

    public TimelineManager Manager { get { return manager; }}
    public AdversaryMover Mover { get { return mover; } }
    private Dictionary<Entity, float> damageTable;
    private Dictionary<Entity, float> threatTable;

    public override void _Ready()
    {
        manager = GetNode<TimelineManager>("%TimeLineManager");
        mover = GetNode<AdversaryMover>("%Mover");
        damageTable = new Dictionary<Entity, float>();
        threatTable = new Dictionary<Entity, float>();

        adversaryStatus = (AdversaryStatus)Status;

        adversaryStatus.DamageTaken += OnDamageTaken;
        adversaryStatus.ThreatInflicted += OnThreatTaken;
        adversaryStatus.HealTaken += (heal, entity) => {};
        adversaryStatus.KnockedOut += () => {};
        base._Ready();
    }

    private void CheckAggro()
    {
        if(!manager.IsEngaged)
        {
            //TODO check for players in aggro range.
            manager.Engage();
        }
    }

    public void Defeat()
    {
        //Implement defeat once all adversaries are dead.
    }

    public Entity GetThreat(int position)
    {
        if(threatTable.Count > 0)
        {
            var list = threatTable.ToList();            
            list.Sort((x, y) => y.Value.CompareTo(x.Value));
            return list[position].Key;
        }
        return null;
    }

    private void OnDamageTaken(float damage, Entity entity)
    {
        if(damageTable.ContainsKey(entity))
        {
            damageTable[entity] = damage;
        }
        else
        {
            damageTable.Add(entity, damage);
        }
    }

    private void OnThreatTaken(float threat, Entity entity)
    {
        if(threatTable.ContainsKey(entity))
        {
            threatTable[entity] = threat;
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