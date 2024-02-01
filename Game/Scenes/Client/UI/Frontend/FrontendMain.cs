using Godot;
using System;

public partial class FrontendMain : Control
{
	public static FrontendMain Instance;

	[Export]
	private AnimationPlayer animationPlayer;
	[Export]
	private BigButton connectButton;


	private BigButton connectLiveButton;
	private BigButton connectLocalButton;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(Instance != null)
		{
			Free();
			return;
		}
		Instance = this;
		animationPlayer.Play("Opening");
		VisibilityChanged += () => animationPlayer.Play("Opening");
	}

	private void ConnectToServer(string ip, int port)
	{
		ClientManager.Instance.TempStart(ip, port);
		Visible = false;
	}

	public void SetState(bool state)
	{
		Visible = state;
	}


}
