using Godot;
using DamageNumber = Mdmc.Code.Client.UI.HUD.Misc.DamageNumber;

namespace Mdmc.Code.Game.Realization;

public partial class RealizationManager: Node
{
    public static RealizationManager Instance;
    
    [Export] private Node3D _realizationContainer;

    [ExportGroup("Damage Numbers")] [Export]
    private PackedScene _damageNumberScene;
    [Export] private Control _damageNumberContainer;

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        base._Ready();
    }

    public override void _ExitTree()
    {
        if(Instance == this )
        {
            Instance = null;
        }
        base._ExitTree();
    }

    public RealizationBuilder CreateRealizationBuilder()
    {
        var builder = new RealizationBuilder();
        return builder;
    }

    public void AddRealization(Realization realization)
    {
        _realizationContainer.AddChild(realization);
    }
    
    public void AddDisposable(Node obj)
    {
        _realizationContainer.AddChild(obj);
    }

    public void SpawnDamageNumber(int value, Vector3 position, Color color)
    {
        var instance = (DamageNumber)_damageNumberScene.Instantiate();
        _damageNumberContainer.AddChild(instance);
        instance.Initialize(value.ToString(), color, position);
    }
}