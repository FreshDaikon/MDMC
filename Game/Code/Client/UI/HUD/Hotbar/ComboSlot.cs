using System.Linq;
using Godot;
using Mdmc.Code.System;
using ArenaManager = Mdmc.Code.Game.Arena.ArenaManager;
using GameManager = Mdmc.Code.Game.GameManager;
using PlayerEntity = Mdmc.Code.Game.Entity.Player.PlayerEntity;

namespace Mdmc.Code.Client.UI.HUD.Hotbar;

public partial class ComboSlot : Control
{
	public MD.ContainerSlot ContainerName;
	public int SlotIndex {get; set;}
	
	private Label _comboSlotLabel;
	private TextureRect _comboGlowRect;
	private PlayerEntity _localPlayer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_comboGlowRect = GetNode<TextureRect>("%ComboGlow");
		_comboSlotLabel = GetNode<Label>("%ComboNumber");
		Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		//////////////////////////////////////////
		
		if(_localPlayer == null)
		{
			var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
			if(players == null)
				return;
			_localPlayer = players.ToList().Find(p => p.Name == Multiplayer.GetUniqueId().ToString());			
		}
		else
		{
			Game.Combat.ArsenalSystem.SkillContainer container = _localPlayer.Arsenal.GetSkillContainer(ContainerName);			
		}
	}
}
