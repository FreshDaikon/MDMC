using Godot;

namespace Daikon.Game;

public record EntityState
{
    public double TimeStamp { get; init; }
    public Vector3 Position { get; init; }
    public Vector3 Rotation { get; init; }
}