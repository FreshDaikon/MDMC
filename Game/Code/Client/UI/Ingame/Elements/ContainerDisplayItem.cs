using Godot;
using System;
using System.Linq;
using Daikon.Game;
using Daikon.Helpers;

namespace Mdmc.Code.Client.UI.Ingame.Elements;

public partial class UIContainerItem : Control
{
	[Export] public MD.ContainerSlot AssignedSlot { get; private set; }  

	
	private Label ContainerName;

	private TextureButton ContainerButton;
	private TextureButton Slot1Button;
	private TextureButton Slot2Button;
	private TextureButton Slot3Button;
	private TextureButton Slot4Button;

	
	[Signal]
	public delegate void ContainerSelectedEventHandler(int assignedSlot);
	[Signal]
	public delegate void SlotSelectedEventHandler(int containerSlot, int skillSlot);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ContainerName = GetNode<Label>("%ContainerName");
		
		//Setup Buttons :
		ContainerButton = GetNode<TextureButton>("%ContainerButton");
		Slot1Button = GetNode<TextureButton>("%SlotButton1");
		Slot2Button = GetNode<TextureButton>("%SlotButton2");
		Slot3Button = GetNode<TextureButton>("%SlotButton3");
		Slot4Button = GetNode<TextureButton>("%SlotButton4");

		ContainerButton.Pressed += () => { EmitSignal(SignalName.ContainerSelected, (int)AssignedSlot); };

		Slot1Button.Pressed += () => { EmitSignal(SignalName.SlotSelected, (int)AssignedSlot, 0); };
		Slot2Button.Pressed += () => { EmitSignal(SignalName.SlotSelected, (int)AssignedSlot, 1); };
		Slot3Button.Pressed += () => { EmitSignal(SignalName.SlotSelected, (int)AssignedSlot, 2); };
		Slot4Button.Pressed += () => { EmitSignal(SignalName.SlotSelected, (int)AssignedSlot, 3); };
	}

	public override void _PhysicsProcess(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();

		var localPlayer = players?.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
		
		if(localPlayer == null) return;

		var container = localPlayer.Arsenal.GetSkillContainer(AssignedSlot);
		
		var skill1 = localPlayer.Arsenal.GetSkill(AssignedSlot, 0);
		var skill2 = localPlayer.Arsenal.GetSkill(AssignedSlot, 1);
		var skill3 = localPlayer.Arsenal.GetSkill(AssignedSlot, 2);
		var skill4 = localPlayer.Arsenal.GetSkill(AssignedSlot, 3);

		if (container != null)
		{
			ContainerName.Text = container.Data.Name;
			ContainerButton.TextureNormal = container.Data.Icon;
			ContainerButton.Modulate = new Color("#ffffff");
		}
		else
		{
			ContainerName.Text = "No Container...";
			ContainerButton.Modulate = new Color("#000000");
		}

		if (skill1 != null)
		{
			Slot1Button.TextureNormal = skill1.Data.Icon;
			Slot1Button.Modulate = new Color("#ffffff");
		}
		else
		{
			Slot1Button.Modulate = new Color("#000000");
		}

		if (skill2 != null)
		{
			Slot2Button.TextureNormal = skill2.Data.Icon;
			Slot2Button.Modulate = new Color("#ffffff");
		}
		else
		{
			Slot2Button.Modulate = new Color("#000000");
		}
		
		if (skill3 != null)
		{
			Slot3Button.TextureNormal = skill3.Data.Icon;
			Slot3Button.Modulate = new Color("#ffffff");
		}
		else
		{
			Slot3Button.Modulate = new Color("#000000");
		}
		
		if (skill4 != null)
		{
			Slot4Button.TextureNormal = skill4.Data.Icon;
			Slot4Button.Modulate = new Color("#ffffff");
		}
		else
		{
			Slot4Button.Modulate = new Color("#000000");
		}


	}
}
