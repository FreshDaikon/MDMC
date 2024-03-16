using System;
using System.Linq;
using Godot;
using Mdmc.Code.Game;
using Mdmc.Code.Game.Combat;
using Mdmc.Code.System;

namespace Mdmc.Code.Client.UI.HUD.DamageMeter;

public partial class DamageMeter : Control
{
	// Export:
	[Export] public MD.CombatMessageType MeterType;
	[Export] private PackedScene _entryScene;
	[Export] private VBoxContainer _entryList;
	[Export] private Control _entryContainer;
	[Export] private Label _totalLabel;
	[Export] private Label _typeLabel;
	[Export] private Label _timeLabel;

	public override void _Ready()
	{
		_typeLabel.Text = MeterType.ToString();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		//////////////////////////////////////////

		_entryContainer.Visible = CombatManager.Instance.IsInCombat;
		
		if(!CombatManager.Instance.IsInCombat)
		{
			var oldEntries = _entryList.GetChildren().Where(x => x is DamageMeterEntry).Cast<DamageMeterEntry>().ToList();	
			if(oldEntries.Count > 0)
			{
				foreach(var entry in oldEntries)
				{
					entry.QueueFree();
				}
			}
			return;
		}
				
		if(Input.IsActionJustPressed("ToggleMeter"))
		{
			var type = Mathf.Wrap((int)MeterType + 1, 0, Enum.GetNames(typeof(MD.CombatMessageType)).Length);
			MeterType = (MD.CombatMessageType)type;
			_typeLabel.Text = MeterType.ToString();
			foreach(var entry in _entryList.GetChildren())
			{
				entry.Free();
			}
		}
		var entries = _entryList.GetChildren().Where(x => x is DamageMeterEntry).Cast<DamageMeterEntry>().ToList();		
		foreach(EntityContributionTracker entry in CombatManager.Instance.GetTracker(MeterType))
		{
			if(entries.Any(e => e.EntryId == entry.Id))
				continue;
			var newEntry = (DamageMeterEntry)_entryScene.Instantiate();
			newEntry.EntryId = entry.Id;
			newEntry.BarType = MeterType;
			_entryList.AddChild(newEntry);
		}		
		if(_entryList.GetChildCount() > 0)
		{
			var unsorted = _entryList.GetChildren().Where(x => x is DamageMeterEntry).Cast<DamageMeterEntry>().ToList();
			var sorted = unsorted.OrderBy(x => x.SortValue).Cast<DamageMeterEntry>().ToList();
			for(int i = 0; i < sorted.Count; i++)
			{
				_entryList.MoveChild(sorted[0], i);
			}
		}		
		_totalLabel.Text = MD.FormatDisplayNumber(CombatManager.Instance.GetTotalValue(MeterType));
		TimeSpan time = TimeSpan.FromSeconds(CombatManager.Instance.GetTimeLapsed());
		_timeLabel.Text = time.ToString(@"hh\:mm\:ss");
	}	
}
