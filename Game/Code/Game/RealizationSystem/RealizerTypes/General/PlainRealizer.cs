using Godot;

namespace Mdmc.Code.Game.Realization;

[GlobalClass]
public partial class PlainRealizer: BaseRealizer
{
    public override void Initialize()
    {
        GD.Print("Realize This!");
    }
}