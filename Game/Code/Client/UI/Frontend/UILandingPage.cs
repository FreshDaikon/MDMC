

using System.Collections.Generic;
using Godot;

public partial class UILandingPage : Control
{
    //Status
    private Label WSConnectionLabel;
    private Label GameStatus;
    //Join Code Field
    private LineEdit JoinCodeEdit;
    private LineEdit GeneratedCodeEdit;
    // Buttons 
    private OptionButton ArenaListOptions;
    private Button RequestGameButton;
    private Button JoinGameButton;
    private Button StartServerButton;
    private Button HostHubButton;
    private Button ConnectWSButton;

    private List<Arena> _arenas;


    public override void _Ready()
    {
        WSConnectionLabel = GetNode<Label>("%WSConnectionLabel");
        ArenaListOptions = GetNode<OptionButton>("%ArenaListOptions");
        JoinCodeEdit = GetNode<LineEdit>("%JoinCodeEdit");
        GeneratedCodeEdit = GetNode<LineEdit>("%GeneratedCodeEdit");
        RequestGameButton = GetNode<Button>("%RequestGameButton");
        JoinGameButton = GetNode<Button>("%JoinGameButton");
        StartServerButton = GetNode<Button>("%StartServerButton");
        ConnectWSButton = GetNode<Button>("%ConnectWSButton");

        //Setup Signals:
        RequestGameButton.Pressed += () => RequestGame();
        JoinGameButton.Pressed += () => JoinGame();
        ConnectWSButton.Pressed += () => ConnectWS();

        //Setup Extras
        var args = MD.GetArgs();
        if(args.ContainsKey("localserverenable"))
        {
            StartServerButton.Visible = true;
            StartServerButton.Disabled = false;
        }
        else
        {
            StartServerButton.Visible = false;
            StartServerButton.Disabled = true;
        }         
        CallDeferred(nameof(GetArenaList));
    }

    private void GetArenaList()
    {
        _arenas = DataManager.Instance.GetArenas();
        GD.Print("Arenas to show: " + _arenas.Count);
        if(_arenas.Count > 0)
        {
            foreach(Arena arena in _arenas)
            {
                ArenaListOptions.AddItem(arena.ArenaName);
            }
        }
    }

    public override void _Process(double delta)
    {

        bool hasConnnection = WSManager.Instance.State == MD.WSConnectionState.Open;
        WSConnectionLabel.Text = "WS Connnection :" + WSManager.Instance.State.ToString();
        JoinGameButton.Disabled = !hasConnnection && (JoinCodeEdit.Text == "");
        RequestGameButton.Disabled = !hasConnnection;
        StartServerButton.Disabled = true;

        var gameId = ClientMultiplayerManager.Instance.GetId();

        GeneratedCodeEdit.Visible = gameId == "" ? false : true;
        if(gameId != "")
        {
            GeneratedCodeEdit.Text = gameId;
        }
        base._Process(delta);
    }

    private void RequestGame()
    {
        WSManager.Instance.RequestGame(_arenas[ArenaListOptions.Selected]);
    }

    private void JoinGame()
    {
        WSManager.Instance.JoinGame(JoinCodeEdit.Text);
    }

    private void ConnectWS()
    {        
        WSManager.Instance.ConnectToOrchestrator();
    }

    private void StartLocalServer()
    {
        ClientMultiplayerManager.Instance.StartLocalServer(_arenas[ArenaListOptions.Selected]);
    }

}