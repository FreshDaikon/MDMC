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
        else
        {
            
            var lastPosition = LatestState.position;
            if ((lastPosition - Position).Length() > 2f)
            {
                Position = lastPosition;
            }
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