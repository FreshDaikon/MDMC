using Godot;
using ArenaManager = Mdmc.Code.Game.Arena.ArenaManager;
using GameManager = Mdmc.Code.Game.GameManager;
using PlayerEntity = Mdmc.Code.Game.Entity.Player.PlayerEntity;

namespace Mdmc.Code.Client.UI.HUD.Bars;

public partial class CastBar : Control
{
	//Exported:
	[Export] private ColorRect _castBarRect;
	[Export] private float _castBarWidth;
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
			_animationPlayer.Play("Casting");
		}
		
		if(_localPlayer.Arsenal.IsCasting)
		{
			Visible = true;

			var lapsed = GameManager.Instance.GameClock - _localPlayer.Arsenal.CastingStartTime;
			if(lapsed == 0)
			{
				Visible = false;
				return;
			}
			float left = (float)(lapsed / _localPlayer.Arsenal.CastingTime);
			_castBarRect.Size = new Vector2(Mathf.Clamp(_castBarWidth * left, 0, _castBarWidth) , _castBarRect.Size.Y);
		}
		else
		{
			Visible = false;
		}		
	}
}
