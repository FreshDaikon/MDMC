using System.Linq;
using Daikon.Game;
using Godot;

namespace Mdmc.Code.Client.UI.HUD.UnitFrames;

public partial class UI_NamePlateContainer : Control
{	
	[Export]
	private PackedScene NameplateAsset;

	public override void _PhysicsProcess(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		//////////////////////////////////////////
		
		var entities = ArenaManager.Instance.GetCurrentArena().GetEntities();
		if (entities == null) return;
		
		var namePlates = GetChildren().Where(x => x is Mdmc.Code.Client.UI.HUD.UnitFrames.NamePlate).Cast<Mdmc.Code.Client.UI.HUD.UnitFrames.NamePlate>().ToList();		
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
			var newEntry = (Mdmc.Code.Client.UI.HUD.UnitFrames.NamePlate)NameplateAsset.Instantiate();
			newEntry.Name = entity.Name;
			AddChild(newEntry);
			newEntry.InitializeFrame(entity);
		}
		var unsorted = GetChildren().Where(x => x is Mdmc.Code.Client.UI.HUD.UnitFrames.NamePlate).Cast<Mdmc.Code.Client.UI.HUD.UnitFrames.NamePlate>().ToList();
		var camera = GetViewport().GetCamera3D();
		if(camera == null)
		{
			return;
		}
		var sorted = unsorted.OrderBy(x => ((camera.GlobalPosition - x.GetEntity().Controller.GlobalPosition).Length())).Cast<Mdmc.Code.Client.UI.HUD.UnitFrames.NamePlate>().ToList();
		for(var i = 0; i < sorted.Count; i++)
		{
			MoveChild(sorted[0], i);
		}
	}
}
