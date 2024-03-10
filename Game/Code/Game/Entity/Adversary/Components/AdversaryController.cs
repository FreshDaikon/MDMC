using Mdmc.Code.Game.Entity.Components;

namespace Mdmc.Code.Game.Entity.Adversary.Components;

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