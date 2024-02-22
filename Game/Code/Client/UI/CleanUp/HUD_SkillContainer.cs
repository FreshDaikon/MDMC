using Godot;
using Daikon.Game;
using Daikon.Helpers;

namespace Daikon.Client;

public partial class HUD_SkillContainer : Control
{
	[Export]
	private string containerName;

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
		MD.Log("Container Name : " + containerName);
		CallDeferred(nameof(SetupSkillSlots));	
	}
	public void SetupSkillSlots()
	{
		Slot1.ContainerName = containerName;
		Slot1.SkillSlot = 0;
		Slot2.ContainerName = containerName;
		Slot2.SkillSlot = 1;
		Slot3.ContainerName = containerName;
		Slot3.SkillSlot = 2;
		Slot4.ContainerName = containerName;
		Slot4.SkillSlot = 3;

		//ComboSlots:
		Combo1.ContainerName = containerName;
		Combo1.SlotIndex = 0;
		Combo2.ContainerName = containerName;
		Combo2.SlotIndex = 1;
		Combo3.ContainerName = containerName;
		Combo3.SlotIndex = 2;
		Combo4.ContainerName = containerName;
		Combo4.SlotIndex = 3;
	}

	private bool wasActivated = false;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(ClientMultiplayerManager.Instance.GetStatus() != MultiplayerPeer.ConnectionStatus.Connected)
			return;
		var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
		if(players == null)
			return;
		var localPlayer = players.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());

		if(localPlayer !=  null) 
		{
			if(localPlayer.playerInput.isActivator1Pressed)
				activatorPressed(PlayerArsenal.ContainerNames.Left);			
			else
				activatorDepressed(PlayerArsenal.ContainerNames.Left);			
			if(localPlayer.playerInput.isActivator2Pressed)
				activatorPressed(PlayerArsenal.ContainerNames.Right);
			else
				activatorDepressed(PlayerArsenal.ContainerNames.Right);
			if(localPlayer.playerInput.isActivator3Pressed)
				activatorPressed(PlayerArsenal.ContainerNames.Main);
			else
				activatorDepressed(PlayerArsenal.ContainerNames.Main);
		}
		
		var container = localPlayer.Arsenal.GetSkillContainer(containerName);
		if(container == null)
		{
			return;
		}
		containerIcon.Texture = container.ContainerIcon;
	}
	private void activatorPressed(string container)
	{
		if(container == containerName)
		{
			if(!isPlaying)
			{ 
				animationPlayer.Play("Active");
				isPlaying = true;
			}
		}
	}
	private void activatorDepressed(string container)
	{
		if(container == containerName)
		{
			if(isPlaying)
			{ 
				animationPlayer.Play("RESET");
				isPlaying = false;
			}
		}
	}

}
