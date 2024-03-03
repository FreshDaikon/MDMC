using Godot;
using Daikon.Game;
using Daikon.Helpers;

namespace Daikon.Client;

public partial class HUD_SkillSlot : Control
{
	public int SkillSlot;
	public MD.ContainerSlot ContainerName;

	private TextureProgressBar _gcdBar;
	private TextureProgressBar _ogcdBar;
	private TextureRect _trigger;
	private AnimationPlayer _triggerPlayer;
	private Label _cdTimer;
	private ColorRect _background;
	private TextureRect _icon;
	private TextureRect _iconGlow;
	
	private PlayerEntity localPlayer;
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
		
		if(localPlayer == null)
		{
			var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
			if(players == null)
				return;
			localPlayer = players.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());			
			if(localPlayer != null)
			{
				localPlayer.playerInput.ActionButtonPressed += TriggerTrigger; 
			}
		}
		else
		{
			var skill = localPlayer.Arsenal.GetSkill(ContainerName, SkillSlot); 
			
			_iconGlow.Visible = (skill != null);
			_icon.Visible = (skill != null);  

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

			var gcd = localPlayer.Arsenal.GetArsenalGCD();
			var gcdStartTime = localPlayer.Arsenal.GCDStartTime;
			var gcdLapsed = Mathf.Clamp(GameManager.Instance.GameClock - gcdStartTime, 0, gcd);
			
			switch (skill.TimerType)
			{
				case MD.SkillTimerType.GCD:
					var gcdLeft = gcd - gcdLapsed;
					var gcdPercent = 100 - ((float)gcdLapsed / (float)gcd * 100f);
				{
					var cd = skill.AdjustedCooldown;
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
					_gcdBar.Value = 0;
					break;
			}
			
		}
	}

	private void TriggerTrigger(int container, int slot)
	{
		if ((MD.ContainerSlot)container != ContainerName || slot != SkillSlot) return;
		_triggerPlayer.Stop();
		_triggerPlayer.Play("Trigger");
	}
}
