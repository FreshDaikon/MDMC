using System.Collections.Generic;
using Godot;

namespace Daikon.Game.Mechanics;

public partial class AOE: BaseMechanic
{

    [Export]
    public PackedScene IndicatorScene;

    private List<AOEIndicator> Indicators = new();

    public override void _Ready()
    {
        base._Ready();
        
    }
    
    public override void StartMechanic()
    {
        base.StartMechanic();
        var playerEntities = ArenaManager.Instance.GetCurrentArena().GetPlayers();

        foreach (var player in playerEntities)
        {
            var newAOE = (AOEIndicator)IndicatorScene.Instantiate();
            Indicators.Add(newAOE);
            newAOE.Init(player.Controller.GlobalPosition, 4);
            ArenaManager.Instance.GetCurrentArena().RealizationPool.AddChild(newAOE);
            Rpc(nameof(SpawnIndicatorOnClient), player.Controller.GlobalPosition, 4);
        }
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SpawnIndicatorOnClient(Vector3 position, float radius)
    {
        var newAOE = (AOEIndicator)IndicatorScene.Instantiate();
        Indicators.Add(newAOE);
        newAOE.Init(position, 4);
        ArenaManager.Instance.GetCurrentArena().RealizationPool.AddChild(newAOE);
   }
    
    public override void ResolveMechanic()
    {
        foreach (var aoe in Indicators)
        {
            aoe.QueueFree();
        }
        base.ResolveMechanic();
    }
}