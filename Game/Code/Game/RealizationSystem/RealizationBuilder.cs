using Godot;

namespace Mdmc.Code.Game.Realization;

public class RealizationBuilder
{

    private Realization _realization;

    public RealizationBuilder New(PackedScene scene, float lifetime)
    {
        _realization = new Realization
        {
            Scene = scene,
            Lifetime = lifetime,            
        };
        return this;
    }

    public RealizationBuilder WithStartPosition(Vector3 position)
    {
        _realization.StartPosition = position;
        return this;
    }

    public RealizationBuilder InTransform(Node3D rootTransform)
    {
        _realization.RootTransform = rootTransform;
        return this;
    }

    public RealizationBuilder WithTarget(Node3D target, float speed)
    {
        _realization.TargetObject = target;
        _realization.Speed = speed;
        return this;
    }

    public RealizationBuilder WithTargetPosition(Vector3 target, float speed)
    {
        _realization.TargetPosition = target;
        _realization.Speed = speed;
        return this;
    }

    public RealizationBuilder WithOffset(Vector3 offset)
    {
        _realization.Offset = offset;
        return this;
    }
    
    public RealizationBuilder WithLookatTarget(Vector3 target)
    {
        _realization.LookatTarget = target;
        return this;
    }

    public RealizationBuilder WithSize(Vector3 size)
    {
        _realization.Size = size;
        return this;
    }
    
    public RealizationBuilder WithRadius(float radius)
    {
        _realization.Radius = radius;
        return this;
    }

    public Realization Spawn()
    {
        _realization.SpawnRealization();
        return _realization;
    }
}