using Godot;

namespace Daikon.Game;

public partial class AdversaryController : EntityController
{
    public override void UpdatePosition()
    {
       Position = SavedPosition;

    }
    public override void UpdateRotation()
    {
        Rotation = SavedRotation;
    }
}