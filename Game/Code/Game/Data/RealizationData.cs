using Godot;

namespace Mdmc.Code.Game.Data;

[GlobalClass]
public abstract partial class RealizationData: Resource
{
    [ExportGroup("Scene to Load:")]
    [Export]
    public PackedScene Scene;
    
    public abstract Realization.Realization GetRealization();
}