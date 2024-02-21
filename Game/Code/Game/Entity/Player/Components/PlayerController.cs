using Godot;

namespace Daikon.Game;

public partial class PlayerController : EntityController
{
    private PlayerEntity player;

    public override void _Ready()
    {
        base._Ready();
        player = (PlayerEntity)GetParent();
    }

    public override void UpdatePosition()
    {
       Position = SavedPosition;

    }
    public override void UpdateRotation()
    {
        if(!player.IsLocalPlayer)
        {
            Rotation = SavedRotation;
        }
    }
}