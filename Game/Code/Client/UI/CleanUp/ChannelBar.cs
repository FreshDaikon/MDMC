using Godot;
using Daikon.Game;
using Daikon.Helpers;

namespace Daikon.Client;

public partial class ChannelBar : Control
{
	[Export]
	private ColorRect ChannelBarRect;
	[Export]
	private float ChannelBarWidth;
	[Export]
	private AnimationPlayer animationPlayer;
	
	private PlayerEntity localPlayer;

	private float[] WeightedValue;

    public override void _Process(double delta)
	{
		if(!GameManager.Instance.IsGameRunning())
			return;
		//////////////////////////////////////////
		var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
		if(players == null)
			return;
		localPlayer = players.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
		if(localPlayer ==  null)
			return;

		if(WeightedValue == null)
		{
			WeightedValue = localPlayer.Arsenal.GetWeightedTotal(localPlayer.Arsenal.GetArsenalSkillWeights());
		}			
		if(!animationPlayer.IsPlaying())
		{
			animationPlayer.Play("Channeling");
		}
		ChannelBarRect.Color = MD.GetPlayerColor(WeightedValue);
		if(localPlayer.Arsenal.IsChanneling)
		{
			Visible = true;
			var lapsed = GameManager.Instance.GameClock - localPlayer.Arsenal.ChannelingStartTime;
			if(lapsed <= 0f)
			{
				Visible = false;
				return;
			}
			float left = ((float)lapsed / (float)localPlayer.Arsenal.ChannelingTime);
			ChannelBarRect.Size = new Vector2(Mathf.Clamp(ChannelBarWidth * (1f - left), 0, ChannelBarWidth) , ChannelBarRect.Size.Y);
		}
		else
		{
			Visible = false;
		}
	}
}
