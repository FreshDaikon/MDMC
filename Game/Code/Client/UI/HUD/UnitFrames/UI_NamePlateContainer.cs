using Daikon.Game;
using Godot;
using System;
using System.Linq;

namespace Daikon.Client;

public partial class UI_NamePlateContainer : Control
{	
	[Export]
	private PackedScene NameplateAsset;

	public override void _PhysicsProcess(double delta)
	{
		var entities = ArenaManager.Instance.GetCurrentArena().GetEntities();
		if(entities != null)
		{  
			var namePlates = GetChildren().Where(x => x is UI_NamePlate).Cast<UI_NamePlate>().ToList();		
            foreach(UI_NamePlate plate in namePlates)
            {
                if(!entities.Any(n => n == plate.GetEntity()))
                {
                    plate.QueueFree();
                }
            }
			foreach(Entity entity in entities)
			{
				if(namePlates.Any(e => e.GetEntity() == entity))
                {
					continue;
                }
				var newEntry = (UI_NamePlate)NameplateAsset.Instantiate();
                newEntry.Name = entity.Name;
				AddChild(newEntry);
				newEntry.InitializeFrame(entity);
			}
            var unsorted = GetChildren().Where(x => x is UI_NamePlate).Cast<UI_NamePlate>().ToList();
            var camera = UIHUDMain.Instance.activeCamera;
            if(camera == null)
            {
                return;
            }
			var sorted = unsorted.OrderBy(x => ((camera.GlobalPosition - x.GetEntity().Controller.GlobalPosition).Length())).Cast<UI_NamePlate>().ToList();
			for(int i = 0; i < sorted.Count; i++)
			{
				MoveChild(sorted[0], i);
			}
		}
	}
}
