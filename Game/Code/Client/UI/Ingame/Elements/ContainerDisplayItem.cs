using System.Linq;
using Godot;
using Mdmc.Code.System;
using ArenaManager = Mdmc.Code.Game.Arena.ArenaManager;
using GameManager = Mdmc.Code.Game.GameManager;

namespace Mdmc.Code.Client.UI.Ingame.Elements;

public partial class ContainerDisplayItem : Control
{
	// Exports :
	[Export] public MD.ContainerSlot AssignedSlot { get; private set; }  
	[Export] private Label _containerName;
	[Export] private TextureButton _containerButton;
	[Export] private TextureButton _slot1Button;
	[Export] private TextureButton _slot2Button;
	[Export] private TextureButton _slot3Button;
	[Export] private TextureButton _slot4Button;

	// Signals:
	[Signal] public delegate void ContainerSelectedEventHandler(int assignedSlot);
	[Signal] public delegate void SlotSelectedEventHandler(int containerSlot, int skillSlot);
	
	public override void _Ready()
	{
		_containerButton.Pressed += () => { EmitSignal(SignalName.ContainerSelected, (int)AssignedSlot); };
		_slot1Button.Pressed += () => { EmitSignal(SignalName.SlotSelected, (int)AssignedSlot, 0); };
		_slot2Button.Pressed += () => { EmitSignal(SignalName.SlotSelected, (int)AssignedSlot, 1); };
		_slot3Button.Pressed += () => { EmitSignal(SignalName.SlotSelected, (int)AssignedSlot, 2); };
		_slot4Button.Pressed += () => { EmitSignal(SignalName.SlotSelected, (int)AssignedSlot, 3); };
	}

	public override void _PhysicsProcess(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();

		var localPlayer = players.ToList().Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
		
		if(localPlayer == null) return;

		var container = localPlayer.Arsenal.GetSkillContainer(AssignedSlot);
		
		var skill1 = localPlayer.Arsenal.GetSkill(AssignedSlot, 0);
		var skill2 = localPlayer.Arsenal.GetSkill(AssignedSlot, 1);
		var skill3 = localPlayer.Arsenal.GetSkill(AssignedSlot, 2);
		var skill4 = localPlayer.Arsenal.GetSkill(AssignedSlot, 3);

		if (container != null)
		{
			_containerName.Text = container.Data.Name;
			_containerButton.TextureNormal = container.Data.Icon;
			_containerButton.Modulate = new Color("#ffffff");
		}
		else
		{
			_containerName.Text = "No Container...";
			_containerButton.Modulate = new Color("#000000");
		}

		if (skill1 != null)
		{
			_slot1Button.TextureNormal = skill1.Data.Icon;
			_slot1Button.Modulate = new Color("#ffffff");
		}
		else
		{
			_slot1Button.Modulate = new Color("#000000");
		}

		if (skill2 != null)
		{
			_slot2Button.TextureNormal = skill2.Data.Icon;
			_slot2Button.Modulate = new Color("#ffffff");
		}
		else
		{
			_slot2Button.Modulate = new Color("#000000");
		}
		
		if (skill3 != null)
		{
			_slot3Button.TextureNormal = skill3.Data.Icon;
			_slot3Button.Modulate = new Color("#ffffff");
		}
		else
		{
			_slot3Button.Modulate = new Color("#000000");
		}
		
		if (skill4 != null)
		{
			_slot4Button.TextureNormal = skill4.Data.Icon;
			_slot4Button.Modulate = new Color("#ffffff");
		}
		else
		{
			_slot4Button.Modulate = new Color("#000000");
		}


	}
}
