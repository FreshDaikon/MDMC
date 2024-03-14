using System.Collections.Generic;
using System.Linq;
using Godot;
using Mdmc.Code.Game.Entity.Player;
using Mdmc.Code.Game.Entity.Player.Components;
using Mdmc.Code.Game.Realization;
using Mdmc.Code.Game.Realization.Realizations.Boss.Mechanics;

namespace Mdmc.Code.Game.Entity.Adversary.Actions.Mechanics;

public partial class SpawnCircleAoe: BaseMechanic
{
    [Export] private PackedScene CircleAoeScene;
    [Export] private float _radius = 4;
    [Export] private int _damage = 1000;

    private ulong _startTime;
    private List<Area3D> _detectors = new();
    private List<PlayerEntity> _players = new ();
        
    
    internal protected override void StartMechanic()
    {
        base.StartMechanic();
        var playerEntities = Mdmc.Code.Game.Arena.ArenaManager.Instance.GetCurrentArena().GetPlayers();
        _startTime = Time.GetTicksMsec();
        
        manager.Entity.StartCast(_resolveTime);
        
        foreach (var player in playerEntities)
        {
            Rpc(nameof(SpawnIndicatorOnClient), player.Controller.GlobalPosition);
            var detector = new Area3D();
            RealizationManager.Instance?.AddDisposable(detector);
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
    private void SpawnIndicatorOnClient(Vector3 position)
    {
        var builder = RealizationManager.Instance.CreateRealizationBuilder();
        var real = builder.New(CircleAoeScene, _resolveTime)
            .WithRadius(_radius)
            .WithStartPosition(position)
            .Spawn();
    }
    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SpawnDamageNumber(Vector3 position, int value)
    {
        RealizationManager.Instance.SpawnDamageNumber(value, position, new Color("#ff0000"));
    }
}