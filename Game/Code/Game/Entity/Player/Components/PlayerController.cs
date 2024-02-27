using Godot;

namespace Daikon.Game;

public partial class PlayerController : EntityController
{
    private PlayerEntity player;

    public override void _Ready()
    {
        player = (PlayerEntity)GetParent();
    }

    public override void UpdatePosition()
    {
        if(!player.IsLocalPlayer)
        {
            Position = SavedPosition;
        }
        else if((Position - SavedPosition).Length() > 1.5f)
        {
            Position = SavedPosition;
        }

    }
    public override void UpdateRotation()
    {
        if(!player.IsLocalPlayer)
        {
            Rotation = SavedRotation;
        }
    }
}