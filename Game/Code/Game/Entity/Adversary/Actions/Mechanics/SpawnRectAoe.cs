using System.Collections.Generic;
using System.Linq;
using Godot;
using Mdmc.Code.Game.Entity.Player;
using Mdmc.Code.Game.Entity.Player.Components;
using Mdmc.Code.Game.RealizationSystem;

namespace Mdmc.Code.Game.Entity.Adversary.Actions.Mechanics;

public partial class SpawnRectAoe: BaseMechanic
{
    [Export] private PackedScene _rectAoeScene;
    [Export] private Vector3 _size = new Vector3(1f, 1f, 1f);
    [Export] private int _damage = 1000;

    private ulong _startTime;
    private List<Area3D> _detectors = new();
    private List<PlayerEntity> _players = new ();
    
    protected internal override void StartMechanic()
    {
        base.StartMechanic();
        var playerEntities = Mdmc.Code.Game.Arena.ArenaManager.Instance.GetCurrentArena().GetPlayers();
        _startTime = Time.GetTicksMsec();
        
        manager.Entity.StartCast(_resolveTime);
        
        foreach (var player in playerEntities)
        {
            var startPosition = manager.Entity.Controller.GlobalPosition;
            var targetPosition = player.Controller.GlobalPosition;
            Rpc(nameof(SpawnIndicatorOnClient), startPosition, targetPosition);
            var detector = new Area3D();
            RealizationManager.Instance?.AddDisposable(detector);
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
            Rpc(nameof(SpawnDamageNumber), player.Controller.GlobalPosition + new Vector3(0f, player.EntityHeight, 0f), _damage);
        }
        foreach (var detector in _detectors.ToList())
        {
            _detectors.Remove(detector);
            detector.QueueFree();
        }
        base.ResolveMechanic();
    }
    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SpawnIndicatorOnClient(Vector3 position, Vector3 target)
    {
        var builder = RealizationManager.Instance.CreateRealizationBuilder();
        var real = builder.New(_rectAoeScene, _resolveTime)
            .WithSize(_size)
            .WithLookatTarget(target)
            .WithStartPosition(position)
            .Spawn();
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SpawnDamageNumber(Vector3 position, int value)
    {
        RealizationManager.Instance.SpawnDamageNumber(value, position, new Color("#ff0000"));
    }
}