 using Godot;
using Mdmc.Code.Game.Realization;
using Steamworks.Data;

public partial class RealizationBuilder
 {

    private Realization realization;

    public RealizationBuilder New(PackedScene scene, float lifetime)
    {
        realization = new Realization
        {
            Scene = scene,
            Lifetime = lifetime,            
        };
        return this;
    }

    public RealizationBuilder WithStartPosition(Vector3 position)
    {
        realization.StartPosition = position;
        return this;
    }

    public RealizationBuilder InTransform(Node3D rootTransform, Vector3 offset)
    {
        realization.RootTransform = rootTransform;
        realization.StartPosition = offset;
        return this;
    }

    public RealizationBuilder WithTarget(Node3D target, float speed)
    {
        realization.TargetObject = target;
        realization.Speed = speed;
        return this;
    }

    public RealizationBuilder WithTargetPosition(Vector3 target, float speed)
    {
        realization.TargetPosition = target;
        realization.Speed = speed;
        return this;
    }
    
    public RealizationBuilder WithLookatTarget(Vector3 target)
    {
        realization.LookatTarget = target;
        return this;
    }

    public RealizationBuilder WithSize(Vector3 size)
    {
        realization.Size = size;
        return this;
    }
    
    public RealizationBuilder WithRadius(float radius)
    {
        realization.Radius = radius;
        return this;
    }

    public Realization Spawn()
    {
        realization.SpawnRealization();
        return realization;
    }

 }