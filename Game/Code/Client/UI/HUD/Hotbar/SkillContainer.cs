using System.Linq;
using Godot;
using Mdmc.Code.System;
using ArenaManager = Mdmc.Code.Game.Arena.ArenaManager;
using GameManager = Mdmc.Code.Game.GameManager;

namespace Mdmc.Code.Client.UI.HUD.Hotbar;

public partial class SkillContainer : Control
{
	[Export]
	private MD.ContainerSlot _containerSlot;

	private string[] _cancels;
	private TextureRect _containerIcon;

	private SkillSlot _slot1;
	private SkillSlot _slot2;
	private SkillSlot _slot3;
	private SkillSlot _slot4;

	private AnimationPlayer _animationPlayer;
	private bool _playerUpdate = false;
	private bool _isPlaying = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_slot1 = GetNode<SkillSlot>("%Slot1");
		_slot2 = GetNode<SkillSlot>("%Slot2");
		_slot3 = GetNode<SkillSlot>("%Slot3");
		_slot4 = GetNode<SkillSlot>("%Slot4");
		_containerIcon = GetNode<TextureRect>("%ContainerIcon");
		_animationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
		//Set Skills Up:	
		GD.Print("Container Name : " + _containerSlot);
		CallDeferred(nameof(SetupSkillSlots));	
	}
	public void SetupSkillSlots()
	{
		_slot1.ContainerName = _containerSlot;
		_slot1.Slot = 0;
		_slot2.ContainerName = _containerSlot;
		_slot2.Slot = 1;
		_slot3.ContainerName = _containerSlot;
		_slot3.Slot = 2;
		_slot4.ContainerName = _containerSlot;
		_slot4.Slot = 3;
	}

	public override void _Process(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		//////////////////////////////////////////
		
		var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
		if(players == null)
			return;
		var localPlayer = players.ToList().Find(p => p.Name == Multiplayer.GetUniqueId().ToString());

		if(localPlayer !=  null) 
		{
			if(localPlayer.Input.isActivator1Pressed)
				activatorPressed(MD.ContainerSlot.Left);			
			else
				activatorDepressed(MD.ContainerSlot.Left);			
			if(localPlayer.Input.isActivator2Pressed)
				activatorPressed(MD.ContainerSlot.Right);
			else
				activatorDepressed(MD.ContainerSlot.Right);
			if(localPlayer.Input.isActivator3Pressed)
				activatorPressed(MD.ContainerSlot.Main);
			else
				activatorDepressed(MD.ContainerSlot.Main);
		}
		
		var container = localPlayer.Arsenal.GetSkillContainer(_containerSlot);
		if(container == null)
		{
			return;
		}
		_containerIcon.Texture = container.Data.Icon;
	}
	private void activatorPressed(MD.ContainerSlot container)
	{
		if(container == _containerSlot)
		{
			if(!_isPlaying)
			{ 
				_animationPlayer.Play("Active");
				_isPlaying = true;
			}
		}
	}
	private void activatorDepressed(MD.ContainerSlot container)
	{
		if(container == _containerSlot)
		{
			if(_isPlaying)
			{ 
				_animationPlayer.Play("RESET");
				_isPlaying = false;
			}
		}
	}

}
