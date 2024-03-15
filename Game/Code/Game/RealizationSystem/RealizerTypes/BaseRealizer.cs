using Godot;
using Mdmc.Code.Game.Realization;

[GlobalClass]
public partial class BaseRealizer: Node3D
{
    public Realization ParentRealization { get; set; }
    
    public virtual void Initialize(){}
}