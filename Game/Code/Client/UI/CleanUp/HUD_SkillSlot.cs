using Godot;
using Daikon.Game;
using Daikon.Helpers;

namespace Daikon.Client;

public partial class HUD_SkillSlot : Control
{
	public int SkillSlot;
	public string ContainerName;

	[Export(PropertyHint.File)]
	public string gcdPath;
	[Export(PropertyHint.File)]
	public string ogcdPath;
	public TextureProgressBar GCDBar;
	public TextureProgressBar OGCDBar;
	public TextureRect Trigger;
	public AnimationPlayer TriggerPlayer;
	public Label CDTimer;
	public ColorRect Background;	
	private PlayerEntity localPlayer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GCDBar = GetNode<TextureProgressBar>("%GCD");
		OGCDBar = GetNode<TextureProgressBar>("%OGCD");
		Trigger = GetNode<TextureRect>("%Trigger");
		TriggerPlayer = GetNode<AnimationPlayer>("%TriggerPlayer");
		Background = GetNode<ColorRect>("%BG");
		CDTimer = GetNode<Label>("%CDTimer");	
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//if(GetTree().Get;
		if(ClientMultiplayerManager.Instance.GetStatus() != MultiplayerPeer.ConnectionStatus.Connected)
			return;
		if(localPlayer == null)
		{
			var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
			if(players == null)
				return;
			localPlayer = players.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());			
			if(localPlayer != null)
			{
				localPlayer.playerInput.ActionButtonPressed += (container, slot) => TriggerTrigger(container, slot); 
			}
		}
		else
		{
			var skill = localPlayer.Arsenal.GetSkill(ContainerName, SkillSlot);
			if(skill == null)
			{
				return;
			}
			//Color Background:			
			CDTimer.Visible = skill.Cooldown > 0 && (skill.Cooldown - ((GameManager.Instance.ServerTick - skill.StartTime) / 1000f) > 0f);
			Background.Color = skill.SkillType switch
			{
				MD.SkillType.TANK => new Color("#734ecf"),
				MD.SkillType.DPS => new Color("#bf3956"),
				MD.SkillType.HEAL => new Color("#7cc468"),
				_ => new Color(0.0f, 0.0f, 0.0f)
			};
			if(skill.IsUniversalSkill)
			{
				Background.Color = new Color("#bfb37a");
			}
			if(skill.TimerType == MD.SkillTimerType.GCD)
			{
				var GCD = localPlayer.Arsenal.GetArsenalGCD();
				var GCDStartTime = localPlayer.Arsenal.GCDStartTime;		
				var GCDLapsed = Mathf.Clamp((GameManager.Instance.ServerTick - GCDStartTime) / 1000f, 0, GCD);	
				var GCDLeft = GCD - GCDLapsed;				
				var GCDPercent = 100 - ((float)GCDLapsed / (float)GCD * 100f);

				if(skill.Cooldown > 0f)
				{
					var CD = skill.Cooldown;
					var CDStartTime = skill.StartTime;
					var CDLapsed = Mathf.Clamp((GameManager.Instance.ServerTick - CDStartTime) / 1000f, 0, CD);
					var CDPercent = 100 - ((float)CDLapsed / (float)CD * 100f);
					var CDLeft = CD - CDLapsed;
					CDTimer.Text = (skill.Cooldown - CDLapsed).ToString((skill.Cooldown - CDLapsed) < 5 ? "0.0" : "0");
					
					var highest = CDLeft > GCDLeft ? CDPercent : GCDPercent;
					GCDBar.Value = highest;
				}
				else
				{
					GCDBar.Value = 100 - ((float)GCDLapsed / (float)GCD * 100f);
				}

			}
			else if(skill.TimerType == MD.SkillTimerType.OGCD)
			{

				var startTime = skill.StartTime;
				var lapsed = (GameManager.Instance.ServerTick - startTime) / 1000f;
				CDTimer.Text = (skill.Cooldown - lapsed).ToString((skill.Cooldown - lapsed) < 5 ? "0.0" : "0");
				GCDBar.Value = 100 - ((float)lapsed / (float)skill.Cooldown * 100f);
			}
			else
			{
				GCDBar.Value = 0;
			}
			
		}
	}

	private void TriggerTrigger(string container, int slot)
	{
		if(container == ContainerName && slot == SkillSlot)
		{
			TriggerPlayer.Stop();
			TriggerPlayer.Play("Trigger");
		}
	}
}
