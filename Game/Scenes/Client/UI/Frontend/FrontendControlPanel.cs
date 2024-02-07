using Godot;
using Steamworks;
using System;

public partial class FrontendControlPanel : Control
{
	// Local:
	private Button connectLocalButton;
	// Remote:
	private LineEdit JoinCode;
	private Button RequestServerButton;
	private Button JoinServerButton;
	//temp stuff:
	private const int PORT = 8080;
	private const string localIP = "127.0.0.1";
    private string onlineIP="194.163.183.217";
	private Arena arena;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Local:
		connectLocalButton = GetNode<Button>("%LocalButton");
		// Remote:
		JoinCode = GetNode<LineEdit>("%JoinCode");
		RequestServerButton = GetNode<Button>("%RequestServer");
		JoinServerButton = GetNode<Button>("%JoinServer");

		//Connect:
		connectLocalButton.Pressed += () => ConnectLocal();
		RequestServerButton.Pressed +=() => WSClient.Instance.RequestGame(arena);
		JoinServerButton.Pressed += () => WSClient.Instance.JoinServer(JoinCode.Text);
	} 
	private void ConnectLocal()
	{
		ClientManager.Instance.StartAsClient(localIP, PORT);
	}

}
