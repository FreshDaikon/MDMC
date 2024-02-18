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

		var localPlayer = ArenaManager.Instance.GetCurrentArena().GetPlayers().Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
		if(localPlayer == null)
		{
			wasActivated = false;
			return;
		}
		else
		{
			if(wasActivated == false)
			{
				localPlayer.playerInput.ActivatorPressed += (container) => activatorPressed(container);
				localPlayer.playerInput.ActivatorDepressed += (container) => activatorDepressed(container);
				wasActivated = true;
			}
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
		MD.Log("Event Gotten!");
		if(container == containerName)
		{
			animationPlayer.Play("Active");
		}
	}
	private void activatorDepressed(string container)
	{
		if(container == containerName)
		{
			animationPlayer.Play("RESET");
		}
	}

}
