using Godot;

namespace Mdmc.Scenes.Client.UI.Frontend.Components;

public partial class MDMCTitle : Control
{
	
	[Export]
	private AnimationPlayer animationPlayer;
	
	public override void _Ready()
	{
		animationPlayer.Play("Open");
		animationPlayer.AnimationFinished += (animation) => IdleTime(animation);
	}

	private void IdleTime(string animation)
	{
		if(animation == "Open")
		{
			animationPlayer.Play("Idle");
		}

	}
}