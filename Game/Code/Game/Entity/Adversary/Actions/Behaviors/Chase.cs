
using Godot;

namespace Daikon.Game;

public partial class Chase: BaseBehavior
{


    public override void ProcessBehavior()
    {
        GD.Print("Processing Chase And Hurt! aw yah!");
        Manager.Entity.Mover.SetDirection(Vector3.Zero);
        base.ProcessBehavior();

    }
}
 