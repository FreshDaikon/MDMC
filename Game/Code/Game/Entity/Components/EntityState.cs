using Godot;

namespace Daikon.Game;

public record EntityState
{
    public double timeStamp { get; init; }
    public Vector3 position { get; init; }
    public Vector3 rotation { get; init; }
}
