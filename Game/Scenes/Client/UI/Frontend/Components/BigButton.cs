using Godot;
using System;

public partial class BigButton : TextureButton
{
	
	private string label;

	[Export]
	public string Label
	{
		get { return label; }
		set {
			label = value;
			buttonLabel.Text = value;
		}
	}

	private AnimationPlayer animationPlayer;
	[Export]
	private Label buttonLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
		FocusEntered += () => OnHover();
		FocusExited += () => OnUnhover();
		MouseEntered += () => OnHover();
		MouseExited += () => OnUnhover();
		
	}
	private void OnHover()
	{
		animationPlayer.Play("Hover");

	}
	private void OnUnhover()
	{
		animationPlayer.PlayBackwards("Hover");
	}
}
