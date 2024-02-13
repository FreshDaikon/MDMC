namespace MDMC.Game.Entity;

public interface IEntityMover
{
    void Move(float delta);
    void Rotate(float delta);
    void Push(float delta);
    void Teleport();
}