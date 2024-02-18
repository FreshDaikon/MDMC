using System.Collections.Generic;
using Godot;
using Daikon.Game;
using Daikon.Helpers;
using Daikon.Client.Connect;

namespace Daikon.Client;

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
    private Button JoinLocalServerButton;
    private Button JoinAnyLocalServerButton;
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
        JoinLocalServerButton = GetNode<Button>("%JoinLocalServerButton");
        JoinAnyLocalServerButton = GetNode<Button>("%JoinAnyLocalServer");
        
        //Setup Signals:
        RequestGameButton.Pressed += () => RequestGame();
        JoinGameButton.Pressed += () => JoinGame();
        ConnectWSButton.Pressed += () => ConnectWS();
        //Local Setup:
        StartServerButton.Pressed += () => StartLocalServer();
        JoinLocalServerButton.Pressed += () => JoinLocalServer();
        JoinAnyLocalServerButton.Pressed += () => JoinAnyLocalServer();
        //Setup Extras
        var args = MD.GetArgs();
        if(args.ContainsKey("standalone"))
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
        JoinLocalServerButton.Disabled = !ClientMultiplayerManager.Instance.HasLocalServer;
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
        DaikonConnect.Instance.DaikonRequestGame(_arenas[ArenaListOptions.Selected].Id);
    }

    private void JoinGame()
    {
        //WSManager.Instance.JoinGame(JoinCodeEdit.Text);
    }

    private void ConnectWS()
    {        
        DaikonConnect.Instance.DaikonAuth();
    }

    private void StartLocalServer()
    {
        ClientMultiplayerManager.Instance.StartLocalServer(_arenas[ArenaListOptions.Selected]);
        ArenaManager.Instance.LoadArena(_arenas[ArenaListOptions.Selected].Id);
    }
    private void JoinLocalServer()
    {
        ClientMultiplayerManager.Instance.SetData("127.0.0.1", 8080);
        ClientMultiplayerManager.Instance.StartPeer(); 
        // a bit weary here..
        UIManager.Instance.TrySetUIState(UIManager.UIState.HUD);
    }

    private void JoinAnyLocalServer()
    {
        //Load the Arena:
        ArenaManager.Instance.LoadArena(_arenas[ArenaListOptions.Selected].Id);
        //Set up Multiplayer:
        ClientMultiplayerManager.Instance.SetData("127.0.0.1", 8080);
        ClientMultiplayerManager.Instance.StartPeer(); 
        // a bit weary here..
        UIManager.Instance.TrySetUIState(UIManager.UIState.HUD);
    }
}