using System.Linq;
using Godot;
using ArenaManager = Mdmc.Code.Game.Arena.ArenaManager;
using GameManager = Mdmc.Code.Game.GameManager;
using PlayerEntity = Mdmc.Code.Game.Entity.Player.PlayerEntity;

namespace Mdmc.Code.Client.UI.HUD.UnitFrames;

public partial class PartyFrame : Control
{
	[Export] private PackedScene _unitFrameAsset;
	[Export] private Control _playerContainer;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		//////////////////////////////////////////
		
		var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
		if(players != null)
		{
			var playerFrames = _playerContainer.GetChildren().Where(x => x is UnitFrame).Cast<UnitFrame>().ToList();		
			foreach(UnitFrame frame in playerFrames)
            {
                if(!players.Any(n => n == frame.GetEntity()))
                {
                    frame.QueueFree();
                }
            }
			foreach(PlayerEntity player in players)
			{
				if(playerFrames.Any(e => e.GetEntity() == player))
					continue;
				var newEntry = (UnitFrame)_unitFrameAsset.Instantiate();
				newEntry.SetEntity(player);
				_playerContainer.AddChild(newEntry);
			}
		}
	}
}
