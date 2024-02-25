using Godot;
using Daikon.Game;
using Daikon.Helpers;

namespace Daikon.Client;

public partial class CastBar : Control
{

	[Export]
	private ColorRect CastBarRect;
	[Export]
	private float CastBarWidth;
	[Export]
	private AnimationPlayer animationPlayer;

	private PlayerEntity localPlayer;
	private float WeightedValue = -1f;

	public override void _Process(double delta)
	{
		if(ClientMultiplayerManager.Instance.GetStatus() != MultiplayerPeer.ConnectionStatus.Connected)
			return;
		var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
		if(players == null)
			return;
		localPlayer = players.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
		if(localPlayer ==  null)
			return;
	
		if(!animationPlayer.IsPlaying())
		{
			animationPlayer.Play("Casting");
		}
		if(WeightedValue == -1f)
		{
			WeightedValue = localPlayer.Arsenal.GetWeightedTotal(localPlayer.Arsenal.GetArsenalSkillWeights());
		}
		CastBarRect.Color = MD.GetPlayerColor(WeightedValue);
		if(localPlayer.Arsenal.IsCasting)
		{
			Visible = true;

			var lapsed = GameManager.Instance.GameClock - localPlayer.Arsenal.CastingStartTime;
			if(lapsed == 0)
			{
				Visible = false;
				return;
			}
			float left = ((float)lapsed / (float)localPlayer.Arsenal.CastingTime);
			CastBarRect.Size = new Vector2(Mathf.Clamp(CastBarWidth * left, 0, CastBarWidth) , CastBarRect.Size.Y);
		}
		else
		{
			Visible = false;
		}		
	}
}
