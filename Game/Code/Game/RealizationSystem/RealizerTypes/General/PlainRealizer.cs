using Godot;

namespace Mdmc.Code.Game.RealizationSystem.RealizerTypes.General;

[GlobalClass]
public partial class PlainRealizer: BaseRealizer
{
    public override void Initialize()
    {
        GD.Print("Realize This!");
    }
}