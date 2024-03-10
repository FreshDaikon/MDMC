using Godot;

namespace Mdmc.Code.Game.Entity.Components;

public record EntityState
{
    public double timeStamp { get; init; }
    public Vector3 position { get; init; }
    public Vector3 rotation { get; init; }
}
