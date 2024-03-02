using Daikon.Game;
using Godot;

[GlobalClass]
public abstract partial class RealizationData: Resource
{
    [ExportGroup("Scene to Load:")]
    [Export]
    public PackedScene Scene;
    
    public abstract Realization GetRealization();
}