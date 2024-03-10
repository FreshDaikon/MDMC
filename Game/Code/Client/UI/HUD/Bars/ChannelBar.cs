using Godot;
using ArenaManager = Mdmc.Code.Game.Arena.ArenaManager;
using GameManager = Mdmc.Code.Game.GameManager;
using PlayerEntity = Mdmc.Code.Game.Entity.Player.PlayerEntity;

namespace Mdmc.Code.Client.UI.HUD.Bars;

public partial class ChannelBar : Control
{
	// Exported:
	[Export] private ColorRect _channelBarRect;
	[Export] private float _channelBarWidth;
	[Export] private AnimationPlayer _animationPlayer;
	
	// Internal:
	private PlayerEntity _localPlayer;
	private float[] _weightedValue;

    public override void _Process(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		//////////////////////////////////////////
		var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
		if(players == null)
			return;
		_localPlayer = players.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
		if(_localPlayer ==  null)
			return;

	
		if(!_animationPlayer.IsPlaying())
		{
			_animationPlayer.Play("Channeling");
		}
		if(_localPlayer.Arsenal.IsChanneling)
		{
			Visible = true;
			var lapsed = GameManager.Instance.GameClock - _localPlayer.Arsenal.ChannelingStartTime;
			if(lapsed <= 0f)
			{
				Visible = false;
				return;
			}
			float left = ((float)lapsed / (float)_localPlayer.Arsenal.ChannelingTime);
			_channelBarRect.Size = new Vector2(Mathf.Clamp(_channelBarWidth * (1f - left), 0, _channelBarWidth) , _channelBarRect.Size.Y);
		}
		else
		{
			Visible = false;
		}
	}
}
