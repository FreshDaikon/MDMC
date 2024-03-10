using System.Linq;
using Godot;
using Mdmc.Code.Game;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.Entity.Adversary;

namespace Mdmc.Code.Client.UI.HUD.UnitFrames;

public partial class EnemyList : Control
{
	[Export] private PackedScene _unitFrameAsset;
	[Export] private Control _enemyContainer;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		//////////////////////////////////////////
		
		var enemies = ArenaManager.Instance.GetCurrentArena().GetEnemyEntities();
		if(enemies != null)
		{
			var enemyFrames = _enemyContainer.GetChildren().Where(x => x is UnitFrame).Cast<UnitFrame>().ToList();		
			foreach(AdversaryEntity enemy in enemies)
			{
				if(enemyFrames.Any(e => e.GetEntity() == enemy))
					continue;
				var newEntry = (UnitFrame)_unitFrameAsset.Instantiate();
				newEntry.SetEntity(enemy);
				_enemyContainer.AddChild(newEntry);
			}
		}
	}
}
