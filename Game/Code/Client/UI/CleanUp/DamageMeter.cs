using Godot;
using System;
using System.Linq;
using Daikon.Game;
using Daikon.Helpers;

namespace Daikon.Client;

public partial class DamageMeter : Control
{
	[Export]
	public MD.CombatMessageType MeterType;
	[Export(PropertyHint.File)]
	private string EntryPath;
	[Export]
	private VBoxContainer EntryList;
	[Export]
	private Control EntryContainer;
	[Export]
	private Label TotalLabel;
	[Export]
	private Label TypeLabel;
	[Export]
	private Label TimeLabel;

	private PackedScene EntryRef;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		EntryRef = (PackedScene)ResourceLoader.Load(EntryPath);
		TypeLabel.Text = MeterType.ToString();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		//////////////////////////////////////////

		EntryContainer.Visible = CombatManager.Instance.IsInCombat;
		if(!CombatManager.Instance.IsInCombat)
			return;
				
		if(Input.IsActionJustPressed("ToggleMeter"))
		{
			var type = Mathf.Wrap((int)MeterType + 1, 0, Enum.GetNames(typeof(MD.CombatMessageType)).Length);
			MeterType = (MD.CombatMessageType)type;
			TypeLabel.Text = MeterType.ToString();
			foreach(var entry in EntryList.GetChildren())
			{
				entry.Free();
			}
		}
		var entries = EntryList.GetChildren().Where(x => x is DamageMeterEntry).Cast<DamageMeterEntry>().ToList();		
		foreach(EntityContributionTracker entry in CombatManager.Instance.GetTracker(MeterType))
		{
			if(entries.Any(e => e.EntryId == entry.Id))
				continue;
			var newEntry = (DamageMeterEntry)EntryRef.Instantiate();
			newEntry.EntryId = entry.Id;
			newEntry.BarType = MeterType;
			EntryList.AddChild(newEntry);
		}		
		if(EntryList.GetChildCount() > 0)
		{
			var unsorted = EntryList.GetChildren().Where(x => x is DamageMeterEntry).Cast<DamageMeterEntry>().ToList();
			var sorted = unsorted.OrderBy(x => x.sortValue).Cast<DamageMeterEntry>().ToList();
			for(int i = 0; i < sorted.Count; i++)
			{
				EntryList.MoveChild(sorted[0], i);
			}
		}		
		TotalLabel.Text = MD.FormatDisplayNumber(CombatManager.Instance.GetTotalValue(MeterType));
		TimeSpan time = TimeSpan.FromSeconds(CombatManager.Instance.GetTimeLapsed());
		TimeLabel.Text = time.ToString(@"hh\:mm\:ss");
	}	
}
