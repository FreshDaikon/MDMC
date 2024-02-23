using Godot;
using System;
using System.Linq;
using Daikon.Game;

namespace Daikon.Client;

public partial class UI_EnemyList : Control
{
	[Export]
	private PackedScene UnitFrameAsset;

	private Control EnemyContainer;

	public override void _Ready()
	{
		EnemyContainer = GetNode<Control>("%EnemyContainer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var enemies = ArenaManager.Instance.GetCurrentArena().GetEnemyEntities();
		if(enemies != null)
		{
			var EnemyFrames = EnemyContainer.GetChildren().Where(x => x is UI_UnitFrame).Cast<UI_UnitFrame>().ToList();		
			foreach(AdversaryEntity enemy in enemies)
			{
				if(EnemyFrames.Any(e => e.GetEntity() == enemy))
					continue;
				var newEntry = (UI_UnitFrame)UnitFrameAsset.Instantiate();
				newEntry.SetEntity(enemy);
				EnemyContainer.AddChild(newEntry);
			}
		}
	}
}
