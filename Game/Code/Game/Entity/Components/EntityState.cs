using Godot;

namespace MDMC.Game.Entity;

public record EntityState
{
    public int TimeStamp { get; init; }
    public Vector3 Position { get; init; }
    public Vector3 Rotation { get; init; }
}