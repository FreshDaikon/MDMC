using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Daikon.Game;

public partial class AdversaryEntity : Entity
{
    [Export]
    private Node3D _startPosition;
    [Export]
    private float _aggroRange = 2f;

    private AdversaryStatus _adversaryStatus;

    public TimelineManager Manager { get; private set; }
    public AdversaryMover Mover { get; private set; }

    private Dictionary<Entity, int> _damageTable;
    private Dictionary<Entity, int> _threatTable;

    public AdversaryState CurrentState = AdversaryState.Idle;

    [Signal]    
    public delegate void EngagedEventHandler();       

    public override void _Ready()
    {
        base._Ready();
        Manager = GetNode<TimelineManager>("%TimeLineManager");
        Mover = GetNode<AdversaryMover>("%Mover");

        _damageTable = new Dictionary<Entity, int>();
        _threatTable = new Dictionary<Entity, int>();
        _adversaryStatus = (AdversaryStatus)Status;
        
        _adversaryStatus.KnockedOut += Defeat;
        _adversaryStatus.DamageTaken += OnDamageTaken;
        _adversaryStatus.ThreatInflicted += OnThreatTaken;
        _adversaryStatus.HealTaken += (heal, entity) => {};        
    }

    public override void _PhysicsProcess(double delta)
    {
        if(!Multiplayer.IsServer()) 
            return;
        switch (CurrentState)
        {
            case AdversaryState.Dead:
                return;
            case AdversaryState.Idle:
                CheckAggro();
                break;
        }
    }    

    private void CheckAggro()
    {
        var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
        if (players == null) return;
        foreach (var player in from player in players let distance = (player.Controller.Position - Controller.Position).Length() where distance <= _aggroRange select player)
        {
            EmitSignal(SignalName.Engaged);
            Engage(player);
        }
    }

    private void Defeat()
    {
        CurrentState = AdversaryState.Dead;
        Manager.Stop();
        Visible = false;
        Targetable = false;
        Rpc(nameof(SyncDefeat));
    }

    public void Engage(Entity entity)
    {
        CurrentState = AdversaryState.Engaged;
        Manager.Start();
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
        var list = _threatTable.Where(e => e.Key.Status.CurrentState != EntityStatus.StatusState.KnockedOut).ToList();            
        
        if(list.Count < position+1 || list.Count == 0) return null;
        
        list.Sort((x, y) => y.Value.CompareTo(x.Value));
        return list[position].Key;
    }

    public float GetThreatValue(int position)
    {
        var list = _threatTable.Where(e => e.Key.Status.CurrentState != EntityStatus.StatusState.KnockedOut).ToList();            
        
        if(list.Count < position+1 || list.Count == 0)  return 0f;
               
        list.Sort((x, y) => y.Value.CompareTo(x.Value));
        return list[position].Value;
    }

    private void OnDamageTaken(int damage, Entity entity)
    {
        if(CurrentState == AdversaryState.Idle)
        {
            EmitSignal(SignalName.Engaged);
        }
        if(!_damageTable.TryAdd(entity, damage))
        {
            _damageTable[entity]  += damage;
        }
    }

    private void OnThreatTaken(int threat, Entity entity)
    {
        if(CurrentState == AdversaryState.Idle)
        {
            EmitSignal(SignalName.Engaged);
        }
        if(!_threatTable.TryAdd(entity, threat))
        {
            _threatTable[entity] += threat;
        }
    }

    public void Reset()
    {
        CurrentState = AdversaryState.Idle;
        Manager.Stop();
        Status.Reset();
        _threatTable.Clear();
        _damageTable.Clear();

        Visible = true;
        Targetable = true;
        
        
        Controller.Teleport(_startPosition.Position);
        Rpc(nameof(SyncReset));
    }
}

public enum AdversaryState
{
    Engaged,
    Idle,
    Dead
}