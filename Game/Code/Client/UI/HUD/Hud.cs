using System;
using Godot;
using Mdmc.Code.Game;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.Entity;
using Mdmc.Code.Game.Entity.Player;

namespace Mdmc.Code.Client.UI.HUD;

public partial class Hud : Control
{
	// Exported:
	[Export] private Label _fpsLabel;
	[Export] private Label _latencyLabel;
	[Export] private Label _timeLeftLabel;
	
	// Internals:
	private PlayerEntity _localPlayer;

	public override void _Ready()
	{
		Visible = false;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
		{
			Visible = false;
			return;
		}
		Visible = true;
		_fpsLabel.Text = "FPS [ " + Engine.GetFramesPerSecond().ToString() +" ]";
		_latencyLabel.Text = "Latency  [ " + (GameManager.Instance.GetLatency() * 1000).ToString("0.00") + " ms ]";
		
		var timeLeft = ArenaManager.Instance.GetCurrentArena().GetTimeLeft();
		var span = TimeSpan.FromSeconds(timeLeft);
		
		_timeLeftLabel.Text =" Time Left : 0" + span.Hours.ToString() + ":" + span.Minutes.ToString() + ":" + span.Seconds.ToString();
		if (_localPlayer != null) return;
		
		var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
		
		if(players == null) return;
		
		_localPlayer = players.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
		Visible = _localPlayer != null;
	}
}
