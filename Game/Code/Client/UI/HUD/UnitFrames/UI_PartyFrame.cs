using Daikon.Game;
using Godot;
using System;
using System.Linq;

namespace Daikon.Client;

public partial class UI_PartyFrame : Control
{
	
	[Export]
	private PackedScene UnitFrameAsset;

	private Control PlayerContainer;

	public override void _Ready()
	{
		PlayerContainer = GetNode<Control>("%PlayerContainer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
		if(players != null)
		{
			var playerFrames = PlayerContainer.GetChildren().Where(x => x is UI_UnitFrame).Cast<UI_UnitFrame>().ToList();		
			foreach(PlayerEntity player in players)
			{
				if(playerFrames.Any(e => e.GetEntity() == player))
					continue;
				var newEntry = (UI_UnitFrame)UnitFrameAsset.Instantiate();
				newEntry.SetEntity(player);
				PlayerContainer.AddChild(newEntry);
			}
		}
	}
}
