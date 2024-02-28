using Godot;

namespace Daikon.Game;

public partial class AdversaryMover : Node, IEntityMover
{
    private AdversaryEntity adversary;

    private Vector3 _direction = Vector3.Zero;
    private Vector3 _velocity = Vector3.Zero;


    public override void _Ready()
    {
        adversary = GetParent<AdversaryEntity>();
    }

    public override void _PhysicsProcess(double delta)
    {
        var fDelta = (float)delta;
        Move(fDelta);
    }

    public void SetDirection(Vector3 direction)
    {
        _direction = direction;
    }

    public void Move(float delta)
    {
        if(!Multiplayer.IsServer())
            return;
            
        var controller = adversary.Controller;
        var speed = adversary.Status.GetCurrentSpeed();

        _velocity.X = speed * _direction.X;
        _velocity.Y = speed * _direction.Y;
        _velocity.Z = speed * _direction.Z;

        controller.Velocity = _velocity;
        _direction = Vector3.Zero;
    }

    public void Push(float delta)
    {
        
    }

    public void Rotate(float delta)
    {
        
    }

    public void Teleport(Vector3 position)
    {
        adversary.Controller.Position = position;
    }
}