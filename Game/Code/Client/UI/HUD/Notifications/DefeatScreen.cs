using Daikon.Game;
using Godot;

namespace Mdmc.Code.Client.UI.HUD.Notifications; 

public partial class UI_DefeatScreen : Control
{

	private AnimationPlayer player;

	public override void _Ready()
	{
		player = GetNode<AnimationPlayer>("%DefeatPlayer");
		ArenaManager.Instance.ArenaDefeat += PlayDefeat;
		player.AnimationFinished += (animation) => FadeOut(animation);
	}

	private void PlayDefeat()
	{
		GD.Print("Hello? from player");
		player.Play("Defeat");
	}
	private void FadeOut(string animation)
	{
		if(animation == "Defeat")
		{
			player.Play("FadeOut");
		}
		else
		{
			player.Play("RESET");
		}
	}
}
