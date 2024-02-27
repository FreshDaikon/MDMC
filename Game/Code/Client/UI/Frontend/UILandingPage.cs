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
    //Join Code Field
    private LineEdit JoinCodeEdit;
    // Buttons 
    private OptionButton ArenaListOptions;
    
    private Button RequestGameButton;
    private Button JoinGameButton;
    private Button StartGameButton;
    private Button StartServerButton;
    private Button JoinLocalServerButton;
    private Button ConnectDaikon;

    private List<ArenaObject> _arenas = new();


    public override void _Ready()
    {
        WSConnectionLabel = GetNode<Label>("%WSConnectionLabel");
        RequestGameButton = GetNode<Button>("%RequestGameButton");
        
        ArenaListOptions = GetNode<OptionButton>("%ArenaListOptions");
        JoinCodeEdit = GetNode<LineEdit>("%JoinCodeEdit");
        JoinGameButton = GetNode<Button>("%JoinGameButton");
        StartGameButton = GetNode<Button>("%StartGameButton");


        StartServerButton = GetNode<Button>("%StartServerButton");
        JoinLocalServerButton = GetNode<Button>("%JoinLocal");        

        ConnectDaikon = GetNode<Button>("%ConnectWSButton");


        //Setup Signals:
        RequestGameButton.Pressed += () => RequestGame();
        JoinGameButton.Pressed += () => JoinGame();
        ConnectDaikon.Pressed += () => ConnectWS();
        //Local Setup:
        StartServerButton.Pressed += () => StartLocalServer();
        JoinLocalServerButton.Pressed += () => JoinLocalServer();

        StartGameButton.ButtonUp += () => StartGame();


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
        CallDeferred(nameof(SetupNetworkListeners));
    }

    private void GetArenaList()
    {
        _arenas = DataManager.Instance.GetAllArenas();
        GD.Print("Arenas to show: " + _arenas.Count);
        if(_arenas.Count > 0)
        {
            foreach(ArenaObject arena in _arenas)
            {
                ArenaListOptions.AddItem(arena.Name);
            }
        }
    }

    private void SetupNetworkListeners()
    {
        DaikonConnect.Instance.AuthSuccess += () => {
            GD.Print("We got the auth responese over here!");
        };
        DaikonConnect.Instance.GameCreated += (serverhost, port, joincode) => {
            GD.Print("We got a game Created over here!");
            JoinCodeEdit.Text = joincode;
            ArenaManager.Instance.LoadArena(_arenas[ArenaListOptions.Selected].Id);
            ClientMultiplayerManager.Instance.SetData(serverhost, port);
        };
        DaikonConnect.Instance.GameFound += (host, port) => {
            GD.Print("We have gotten a game here!");
            ArenaManager.Instance.LoadArena(_arenas[ArenaListOptions.Selected].Id);
            ClientMultiplayerManager.Instance.SetData(host, port);
        };
        
    }

    private void RequestGame()
    {
        DaikonConnect.Instance.DaikonRequestGame(_arenas[ArenaListOptions.Selected].Id);
    }

    private void JoinGame()
    {
        GD.Print("Let's see if we can find a game!");
        DaikonConnect.Instance.DaikonJoinGame(JoinCodeEdit.Text);
    }

    private void StartGame()
    {
        ClientMultiplayerManager.Instance.StartPeer();
        UIManager.Instance.TrySetUIState(UIManager.UIState.HUD);
        Multiplayer.ConnectedToServer += () => 
        {
            GameManager.Instance.StartGame(false);
        };
    }

    private void ConnectWS()
    {        
        DaikonConnect.Instance.SetToLocalHost();
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
        Multiplayer.ConnectedToServer += () => 
        {
            GameManager.Instance.StartGame(false);
        };
    }

    private void JoinAnyLocalServer()
    {
        //Load the Arena:
        ArenaManager.Instance.LoadArena(_arenas[ArenaListOptions.Selected].Id);
        //Set up Multiplayer:
        ClientMultiplayerManager.Instance.SetData("127.0.0.1", 8080);
        ClientMultiplayerManager.Instance.StartPeer(); 
        // a bit weary here..
        Multiplayer.ConnectedToServer += () => 
        {
            GameManager.Instance.StartGame(false);
            UIManager.Instance.TrySetUIState(UIManager.UIState.HUD);
        };
    }
}