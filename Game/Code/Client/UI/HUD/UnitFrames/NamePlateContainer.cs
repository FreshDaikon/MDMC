using System.Linq;
using Godot;
using ArenaManager = Mdmc.Code.Game.Arena.ArenaManager;
using GameManager = Mdmc.Code.Game.GameManager;

namespace Mdmc.Code.Client.UI.HUD.UnitFrames;

public partial class NamePlateContainer : Control
{	
	[Export]
	private PackedScene _nameplateAsset;

	public override void _PhysicsProcess(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		//////////////////////////////////////////
		
		var entities = ArenaManager.Instance.GetCurrentArena().GetEntities();
		if (entities == null) return;
		
		var namePlates = GetChildren().Where(x => x is NamePlate).Cast<NamePlate>().ToList();		
		foreach (var plate in namePlates.Where(plate => entities.All(n => n != plate.GetEntity())))
		{
			plate.QueueFree();
		}
		foreach(var entity in entities)
		{
			if(namePlates.Any(e => e.GetEntity() == entity))
			{
				continue;
			}
			var newEntry = (NamePlate)_nameplateAsset.Instantiate();
			newEntry.Name = entity.Name;
			AddChild(newEntry);
			newEntry.InitializeFrame(entity);
		}
		var unsorted = GetChildren().Where(x => x is NamePlate).Cast<NamePlate>().ToList();
		var camera = GetViewport().GetCamera3D();
		if(camera == null)
		{
			return;
		}
		var sorted = unsorted.OrderBy(x => ((camera.GlobalPosition - x.GetEntity().Controller.GlobalPosition).Length())).ToList();
		for(var i = 0; i < sorted.Count; i++)
		{
			MoveChild(sorted[0], i);
		}
	}
}
