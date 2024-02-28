using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Daikon.Game;

public partial class AdversaryEntity : Entity
{
    [Export]
    private Node3D StartPosition;
    [Export]
    private float AggroRange = 2f;

    private TimelineManager manager;
    private AdversaryMover mover;
    private AdversaryStatus adversaryStatus;

    public TimelineManager Manager { get { return manager; }}
    public AdversaryMover Mover { get { return mover; } }

    private Dictionary<Entity, float> damageTable;
    private Dictionary<Entity, float> threatTable;

    public enum AdversaryState
    {
        Engaged,
        Idle,
        Dead
    }

    public AdversaryState CurrentState = AdversaryState.Idle;

    [Signal]    
    public delegate void EngagedEventHandler();       

    public override void _Ready()
    {
        base._Ready();
        manager = GetNode<TimelineManager>("%TimeLineManager");
        mover = GetNode<AdversaryMover>("%Mover");

        damageTable = new Dictionary<Entity, float>();
        threatTable = new Dictionary<Entity, float>();
        adversaryStatus = (AdversaryStatus)Status;
        
        adversaryStatus.KnockedOut += Defeat;
        adversaryStatus.DamageTaken += OnDamageTaken;
        adversaryStatus.ThreatInflicted += OnThreatTaken;
        adversaryStatus.HealTaken += (heal, entity) => {};        
    }

    public override void _PhysicsProcess(double delta)
    {
        if(!Multiplayer.IsServer()) 
            return;
        if(CurrentState == AdversaryState.Dead)
            return;
        if(CurrentState == AdversaryState.Idle)
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
                    EmitSignal(SignalName.Engaged);
                    Engage(player);
                } 
            }
        }
    }

    private void Defeat()
    {
        CurrentState = AdversaryState.Dead;
        manager.Stop();

        Visible = false;
        Targetable = false;

        Rpc(nameof(SyncDefeat));
    }

    public void Engage(Entity entity)
    {
        CurrentState = AdversaryState.Engaged;
        manager.Start();
        OnThreatTaken(500, entity);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncDefeat()
    {
        Visible = false;
        Targetable = false;
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncReset()
    {
        Visible = true;
        Targetable = true;
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
        if(CurrentState == AdversaryState.Idle)
        {
            EmitSignal(SignalName.Engaged);
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
        if(CurrentState == AdversaryState.Idle)
        {
            EmitSignal(SignalName.Engaged);
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
        CurrentState = AdversaryState.Idle;
        manager.Stop();
        Status.Reset();
        threatTable.Clear();
        damageTable.Clear();

        Visible = true;
        Targetable = true;
        
        
        Controller.Teleport(StartPosition.Position);
        Rpc(nameof(SyncReset));
    }
}