using Godot;
using Daikon.Game;

namespace Daikon.Client;

public partial class UIConnections : Control
{
	// Local:
	private Button connectLocalButton;
	private Button StartLocalServerButton;
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
		StartLocalServerButton = GetNode<Button>("%StartLocalServerButton");
		// Remote:
		JoinCode = GetNode<LineEdit>("%JoinCode");
		RequestServerButton = GetNode<Button>("%RequestServer");
		JoinServerButton = GetNode<Button>("%JoinServer");
		//Connect:
		connectLocalButton.Pressed += () => ConnectLocal();
		RequestServerButton.Pressed +=() => WSManager.Instance.RequestGame(arena);
		JoinServerButton.Pressed += () => WSManager.Instance.JoinGame(JoinCode.Text);
	}

    public override void _Process(double delta)
    {
		
        base._Process(delta);
    }
    private void ConnectLocal()
	{
		
	}

	private void StartLocalServer()
	{
		var arena = DataManager.Instance.GetArena(-1);
	}

}