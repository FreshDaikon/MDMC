using Godot;
using Daikon.Game;
using Daikon.Helpers;

namespace Daikon.Client;

public partial class HUD_SkillContainer : Control
{
	[Export]
	private MD.ContainerSlot containerSlot;

	[Export]
	private string[] Activators { get; set; }
	private string[] Cancellors { get; set; }

	private TextureRect containerIcon;

	public HUD_SkillSlot Slot1;
	public HUD_SkillSlot Slot2;
	public HUD_SkillSlot Slot3;
	public HUD_SkillSlot Slot4;

	public HUD_ComboSlot Combo1;
	public HUD_ComboSlot Combo2;
	public HUD_ComboSlot Combo3;
	public HUD_ComboSlot Combo4;

	private AnimationPlayer animationPlayer;
	private bool _playerUpdate = false;

	private bool isPlaying = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Slot1 = GetNode<HUD_SkillSlot>("%Slot1");
		Slot2 = GetNode<HUD_SkillSlot>("%Slot2");
		Slot3 = GetNode<HUD_SkillSlot>("%Slot3");
		Slot4 = GetNode<HUD_SkillSlot>("%Slot4");
		Combo1 = GetNode<HUD_ComboSlot>("%Combo1");
		Combo2 = GetNode<HUD_ComboSlot>("%Combo2");
		Combo3 = GetNode<HUD_ComboSlot>("%Combo3");
		Combo4 = GetNode<HUD_ComboSlot>("%Combo4");
		containerIcon = GetNode<TextureRect>("%ContainerIcon");
		animationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
		//Set Skills Up:	
		GD.Print("Container Name : " + containerSlot);
		CallDeferred(nameof(SetupSkillSlots));	
	}
	public void SetupSkillSlots()
	{
		Slot1.ContainerName = containerSlot;
		Slot1.SkillSlot = 0;
		Slot2.ContainerName = containerSlot;
		Slot2.SkillSlot = 1;
		Slot3.ContainerName = containerSlot;
		Slot3.SkillSlot = 2;
		Slot4.ContainerName = containerSlot;
		Slot4.SkillSlot = 3;
		//ComboSlots:
		Combo1.ContainerName = containerSlot;
		Combo1.SlotIndex = 0;
		Combo2.ContainerName = containerSlot;
		Combo2.SlotIndex = 1;
		Combo3.ContainerName = containerSlot;
		Combo3.SlotIndex = 2;
		Combo4.ContainerName = containerSlot;
		Combo4.SlotIndex = 3;
	}

	private bool wasActivated = false;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		//////////////////////////////////////////
		
		var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
		if(players == null)
			return;
		var localPlayer = players.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());

		if(localPlayer !=  null) 
		{
			if(localPlayer.playerInput.isActivator1Pressed)
				activatorPressed(MD.ContainerSlot.Left);			
			else
				activatorDepressed(MD.ContainerSlot.Left);			
			if(localPlayer.playerInput.isActivator2Pressed)
				activatorPressed(MD.ContainerSlot.Right);
			else
				activatorDepressed(MD.ContainerSlot.Right);
			if(localPlayer.playerInput.isActivator3Pressed)
				activatorPressed(MD.ContainerSlot.Main);
			else
				activatorDepressed(MD.ContainerSlot.Main);
		}
		
		var container = localPlayer.Arsenal.GetSkillContainer(containerSlot);
		if(container == null)
		{
			return;
		}
		containerIcon.Texture = container.Data.Icon;
	}
	private void activatorPressed(MD.ContainerSlot container)
	{
		if(container == containerSlot)
		{
			if(!isPlaying)
			{ 
				animationPlayer.Play("Active");
				isPlaying = true;
			}
		}
	}
	private void activatorDepressed(MD.ContainerSlot container)
	{
		if(container == containerSlot)
		{
			if(isPlaying)
			{ 
				animationPlayer.Play("RESET");
				isPlaying = false;
			}
		}
	}

}
