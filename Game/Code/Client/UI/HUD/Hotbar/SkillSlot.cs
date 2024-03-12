using Godot;
using Mdmc.Code.System;
using ArenaManager = Mdmc.Code.Game.Arena.ArenaManager;
using GameManager = Mdmc.Code.Game.GameManager;
using PlayerEntity = Mdmc.Code.Game.Entity.Player.PlayerEntity;

namespace Mdmc.Code.Client.UI.HUD.Hotbar;

public partial class SkillSlot : Control
{
	public int Slot;
	public MD.ContainerSlot ContainerName;

	private TextureProgressBar _gcdBar;
	private TextureProgressBar _ogcdBar;
	private TextureRect _trigger;
	private AnimationPlayer _triggerPlayer;
	private Label _cdTimer;
	private ColorRect _background;
	private TextureRect _icon;
	private TextureRect _iconGlow;
	private PlayerEntity _localPlayer;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_gcdBar = GetNode<TextureProgressBar>("%GCD");
		_ogcdBar = GetNode<TextureProgressBar>("%OGCD");
		_icon = GetNode<TextureRect>("%Icon");
		_iconGlow = GetNode<TextureRect>("%IconGlow");
		_trigger = GetNode<TextureRect>("%Trigger");
		_triggerPlayer = GetNode<AnimationPlayer>("%TriggerPlayer");
		_background = GetNode<ColorRect>("%BG");
		_cdTimer = GetNode<Label>("%CDTimer");	
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		//////////////////////////////////////////
		
		if(_localPlayer == null)
		{
			var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
			if(players == null)
				return;
			_localPlayer = players.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());			
			if(_localPlayer != null)
			{
				_localPlayer.playerInput.ActionButtonPressed += TriggerTrigger; 
			}
		}
		else
		{
			var skill = _localPlayer.Arsenal.GetSkill(ContainerName, Slot); 
			
			_iconGlow.Visible = skill != null;
			_icon.Visible = skill != null;  

			if (skill == null) return;
			
			_icon.Texture = skill.Data.Icon;
			_iconGlow.Texture = skill.Data.Icon;
			
			//Color Background:			
			_cdTimer.Visible = skill.Cooldown > 0 && (skill.Cooldown - (GameManager.Instance.GameClock - skill.StartTime) > 0f);
			
			_icon.SelfModulate = skill.SkillType switch
			{
				MD.SkillType.TANK => new Color("#0088ff"),
				MD.SkillType.DPS => new Color("#ff2424"),
				MD.SkillType.HEAL => new Color("#2bff24"),
				_ => new Color(0.0f, 0.0f, 0.0f)
			};
			if(skill.IsUniversalSkill)
			{
				_icon.SelfModulate = new Color("#ffd000");
			}

			var gcd = _localPlayer.Arsenal.GetArsenalGCD();
			var gcdStartTime = _localPlayer.Arsenal.GCDStartTime;
			var gcdLapsed = Mathf.Clamp(GameManager.Instance.GameClock - gcdStartTime, 0, gcd);
			
			switch (skill.TimerType)
			{
				case MD.SkillTimerType.GCD:
				{
					var gcdLeft = gcd - gcdLapsed;
					var gcdPercent = 100 - ((float)gcdLapsed / (float)gcd * 100f);
					var cd = skill.Cooldown;
					var cdStartTime = skill.StartTime;
					var cdLapsed = Mathf.Clamp(GameManager.Instance.GameClock - cdStartTime, 0, cd);
					var cdPercent = 100 - cdLapsed / cd * 100f;
					if(skill.Cooldown > 0f)
					{
						var cdLeft = cd - cdLapsed;
						_cdTimer.Text = (skill.Cooldown - cdLapsed).ToString((skill.Cooldown - cdLapsed) < 5 ? "0.0" : "0");
						var highest = cdLeft > gcdLeft ? cdPercent : gcdPercent;
						_gcdBar.Value = highest;
					}
					else
					{
						_gcdBar.Value = 100 - (gcdLapsed / gcd * 100f);
					}
					break;
				}
				case MD.SkillTimerType.OGCD:
				{
					var startTime = skill.StartTime;
					var lapsed = GameManager.Instance.GameClock - startTime;
					_cdTimer.Text = (skill.Cooldown - lapsed).ToString((skill.Cooldown - lapsed) < 5 ? "0.0" : "0");
					_gcdBar.Value = 100 - ((float)lapsed / (float)skill.Cooldown * 100f);
					break;
				}
				default:
				{
					_gcdBar.Value = 0;
					break;
				}
			}
			
		}
	}

	private void TriggerTrigger(int container, int slot)
	{
		if ((MD.ContainerSlot)container != ContainerName || slot != Slot) return;
		_triggerPlayer.Stop();
		_triggerPlayer.Play("Trigger");
	}
}
