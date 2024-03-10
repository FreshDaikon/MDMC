using System;
using Daikon.Game;
using Godot;
using ArenaManager = Mdmc.Code.Game.Arena.ArenaManager;

namespace Mdmc.Code.Client.UI.HUD;

public partial class HudContainer : Control
{

	private PlayerEntity _localPlayer;

	private Label _fpsLabel;
	private Label _latencyLabel;
	private Label _timeLeftLabel;
	public Entity LocalPlayerUiTarget;

	public override void _Ready()
	{
		Visible = false;
		_fpsLabel = GetNode<Label>("%FPS");
		_latencyLabel = GetNode<Label>("%Latency");
		_timeLeftLabel = GetNode<Label>("%TimeLeft");
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
