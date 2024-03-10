using Godot;
using ArenaManager = Mdmc.Code.Game.Arena.ArenaManager;

namespace Mdmc.Code.Client.UI.HUD.Notifications; 

public partial class DefeatScreen : Control
{
	private AnimationPlayer _player;

	public override void _Ready()
	{
		_player = GetNode<AnimationPlayer>("%DefeatPlayer");
		ArenaManager.Instance.ArenaDefeat += PlayDefeat;
		_player.AnimationFinished += (animation) => FadeOut(animation);
	}

	private void PlayDefeat()
	{
		GD.Print("Hello? from player");
		_player.Play("Defeat");
	}
	private void FadeOut(string animation)
	{
		_player.Play(animation == "Defeat" ? "FadeOut" : "RESET");
	}
}
