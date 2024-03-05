using System.Collections.Generic;
using System.Linq;
using Daikon.Game.Realizations.Boss.Mechanics;
using Godot;

namespace Daikon.Game.Mechanics;

public partial class SpawnRectAoe: BaseMechanic
{
    [Export] private RectAoeRealizationData _realizationData;
    [Export] private Vector2 _size = new Vector2(1f, 1f);
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
            var startPosition = manager.Entity.Controller.GlobalPosition;
            var targetPosition = player.Controller.GlobalPosition;
            Rpc(nameof(SpawnIndicatorOnClient), startPosition, targetPosition);
            var detector = new Area3D();
            ArenaManager.Instance.GetCurrentArena().RealizationPool.AddChild(detector);
            detector.Position = startPosition;
            var collision = new CollisionShape3D();
            var shape = new BoxShape3D();
            shape.Size = new Vector3(_size.X, 4f, _size.Y);
            collision.Shape = shape;
            detector.AddChild(collision);
            collision.Position = new Vector3(0f, 0f, -(_size.Y / 2));
            detector.LookAt(targetPosition);
            detector.BodyEntered += OnBodyEntered;
            detector.BodyExited += OnBodyExited;
            _detectors.Add(detector);
        }
    }
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SpawnIndicatorOnClient(Vector3 position, Vector3 target)
    {
        var newAoe = (RectAoeRealization)_realizationData.GetRealization();
        newAoe.SetData(position, target, _resolveTime, _size);
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
            player.Status.InflictDamage(_damage, manager.Entity);
        }
        foreach (var detector in _detectors.ToList())
        {
            _detectors.Remove(detector);
            detector.QueueFree();
        }
        base.ResolveMechanic();
    }
}