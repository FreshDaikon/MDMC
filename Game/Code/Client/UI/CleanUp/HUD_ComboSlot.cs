using Godot;
using Daikon.Game;

namespace Daikon.Client;

public partial class HUD_ComboSlot : Control
{

	public string ContainerName;
	public int SlotIndex {get; set;}
	
	private Label ComboSlotLabel;
	private TextureRect ComboGlowRect;
	private PlayerEntity localPlayer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ComboGlowRect = GetNode<TextureRect>("%ComboGlow");
		ComboSlotLabel = GetNode<Label>("%ComboNumber");
		Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if(ClientMultiplayerManager.Instance.GetStatus() != MultiplayerPeer.ConnectionStatus.Connected)
			return;
		if(localPlayer == null)
		{
			var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
			if(players == null)
				return;
			localPlayer = players.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());			
		}
		else
		{
			SkillContainer container = localPlayer.Arsenal.GetSkillContainer(ContainerName);			
			if(container == null)
				return;
			if(container.SkillSlots == null)
				return;			
			for(int i = 0; i < container.SkillSlots.Length; i++)
			{
				SkillSlotData current = container.SkillSlots[i];
				if(current.ComboSlotIndex == SlotIndex)
				{
					Visible = current.IsComboSlot;
					ComboSlotLabel.Text = (i+1).ToString();
					if(container.NextComboSlot == i)
					{
						ComboGlowRect.Visible = true;
					}
					else
					{
						ComboGlowRect.Visible = false;
					}
					break;
				}
			}
		}
	}
}
