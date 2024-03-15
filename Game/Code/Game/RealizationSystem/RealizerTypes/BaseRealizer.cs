using Godot;

namespace Mdmc.Code.Game.RealizationSystem.RealizerTypes;

[GlobalClass]
public partial class BaseRealizer: Node3D
{
    public Realization ParentRealization { get; set; }
    
    public virtual void Initialize(){}
}