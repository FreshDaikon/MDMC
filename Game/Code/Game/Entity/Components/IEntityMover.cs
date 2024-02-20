using Godot;

namespace Daikon.Game;

public interface IEntityMover
{
    void Move(float delta);
    void Rotate(float delta);
    void Push(float delta);
    void Teleport(Vector3 position);
} 