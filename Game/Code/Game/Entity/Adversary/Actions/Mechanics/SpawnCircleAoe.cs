using System.Collections.Generic;
using System.Linq;
using Daikon.Game.Realizations.Boss.Mechanics;
using Godot;

namespace Daikon.Game.Mechanics;

public partial class SpawnCircleAoe: BaseMechanic
{
    [Export] private CircleAoeRealizationData _realizationData;
    [Export] private float _radius = 4;
    [Export] private int _damage = 1000;

    private ulong _startTime;
    private List<Area3D> _detectors = new();
    private List<PlayerEntity> _players = new ();
        
    
    internal protected override void StartMechanic()
    {
        base.StartMechanic();
        var playerEntities = ArenaManager.Instance.GetCurrentArena().GetPlayers();
        _startTime = Time.GetTicksMsec();
        
        foreach (var player in playerEntities)
        {
            Rpc(nameof(SpawnIndicatorOnClient), player.Controller.GlobalPosition);
            var detector = new Area3D();
            ArenaManager.Instance.GetCurrentArena().RealizationPool.AddChild(detector);
            detector.Position = player.Controller.GlobalPosition;
            var collision = new CollisionShape3D();
            var shape = new CylinderShape3D();
            shape.Radius = _radius;
            collision.Shape = shape;
            detector.AddChild(collision);
            detector.BodyEntered += OnBodyEntered;
            detector.BodyExited += OnBodyExited;
            _detectors.Add(detector);
        }
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SpawnIndicatorOnClient(Vector3 position)
    {
        var newAoe = (CircleAoeRealization)_realizationData.GetRealization();
        newAoe.SetData(position, _radius, _resolveTime);
        newAoe.Spawn();
        
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is not PlayerController) return;
        GD.Print("A player entered the aoe!");
        var entity = body.GetParent<PlayerEntity>();
        if (entity == null)
        {
            GD.Print("I guess it was not a player!");
            return;
        }
        GD.Print("Add Player:" + entity.Name);
        _players.Add(entity);
    }

    private void OnBodyExited(Node3D body)
    {
        if (body is not PlayerController) return;
        GD.Print("A player exited the aoe!");
        var entity = body.GetParent<PlayerEntity>();
        if (entity == null)
        {
            GD.Print("I guess it was not a player!");
            return;
        }
        GD.Print("Remove Player:" + entity.Name);
        _players.Remove(entity);
    }
    
    protected internal override void ResolveMechanic()
    {
        GD.Print("Resolving Mechanic");
        foreach (var player in _players)
        {
            player.Status.InflictDamage(1000, manager.Entity);
        }
        foreach (var detector in _detectors.ToList())
        {
            _detectors.Remove(detector);
            detector.QueueFree();
        }
        base.ResolveMechanic();
    }
}