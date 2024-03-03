using System.Collections.Generic;
using Godot;
using Daikon.Game;
using Daikon.Helpers;
using Daikon.Client.Connect;

namespace Daikon.Client;

public partial class UILandingPage : Control
{
    //Status
    private Label _wsConnectionLabel;
    //Join Code Field
    private LineEdit _joinCodeEdit;
    // Buttons 
    private OptionButton _arenaListOptions;
    
    private Button _requestGameButton;
    private Button _joinGameButton;
    private Button _startGameButton;
    private Button _startServerButton;
    private Button _joinLocalServerButton;
    private Button _connectDaikonLocal;
    private Button _connectDaikonRemote;

    private List<ArenaData> _arenas = new();


    public override void _Ready()
    {
        _wsConnectionLabel = GetNode<Label>("%WSConnectionLabel");
        _requestGameButton = GetNode<Button>("%RequestGameButton");
        
        _arenaListOptions = GetNode<OptionButton>("%ArenaListOptions");
        _joinCodeEdit = GetNode<LineEdit>("%JoinCodeEdit");
        _joinGameButton = GetNode<Button>("%JoinGameButton");
        _startGameButton = GetNode<Button>("%StartGameButton");


        _startServerButton = GetNode<Button>("%StartServerButton");
        _joinLocalServerButton = GetNode<Button>("%JoinLocal");        

        _connectDaikonLocal = GetNode<Button>("%ConnectDaikonLocal");
        _connectDaikonRemote = GetNode<Button>("%ConnectDaikonRemote");


        //Setup Signals:
        _requestGameButton.Pressed += RequestGame;
        _joinGameButton.Pressed += JoinGame;
        _connectDaikonLocal.Pressed += () => ConnectToDaikon(false);
        _connectDaikonRemote.Pressed += () => ConnectToDaikon(true);
        //Local Setup:
        _startServerButton.Pressed += StartLocalServer;
        _joinLocalServerButton.Pressed += JoinLocalServer;

        _startGameButton.ButtonUp += StartGame;
      
        CallDeferred(nameof(GetArenaList));
        CallDeferred(nameof(SetupNetworkListeners));

    }

    private void GetArenaList()
    {
        _arenas = DataManager.Instance.GetAllArenas();
        GD.Print("Arenas to show: " + _arenas.Count);
        if(_arenas.Count > 0)
        {
            foreach(ArenaData arena in _arenas)
            {
                _arenaListOptions.AddItem(arena.Name);
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
            _joinCodeEdit.Text = joincode;
            ArenaManager.Instance.LoadArena(_arenas[_arenaListOptions.Selected].Id);
            ClientMultiplayerManager.Instance.SetData(serverhost, port);
        };
        DaikonConnect.Instance.GameFound += (host, port) => {
            GD.Print("We have gotten a game here!");
            ArenaManager.Instance.LoadArena(_arenas[_arenaListOptions.Selected].Id);
            ClientMultiplayerManager.Instance.SetData(host, port);
        };
        
    }

    private void RequestGame()
    {
        DaikonConnect.Instance.DaikonRequestGame(_arenas[_arenaListOptions.Selected].Id);
    }

    private void JoinGame()
    {
        GD.Print("Let's see if we can find a game!");
        if(ArenaManager.Instance.GetCurrentArena() == null)
        {
            ArenaManager.Instance.LoadArena(_arenas[_arenaListOptions.Selected].Id);
        }
        DaikonConnect.Instance.DaikonJoinGame(_joinCodeEdit.Text);
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

    private void ConnectToDaikon(bool remote)
    {        
        if(remote)
        {
            DaikonConnect.Instance.SetToRemote();
        }
        else
        {
            DaikonConnect.Instance.SetToLocalHost();
        }
        DaikonConnect.Instance.DaikonAuthenticate();
    }

    private void StartLocalServer()
    {
        ClientMultiplayerManager.Instance.StartLocalServer(_arenas[_arenaListOptions.Selected]);
        ArenaManager.Instance.LoadArena(_arenas[_arenaListOptions.Selected].Id);
    }
    private void JoinLocalServer()
    {
        ClientMultiplayerManager.Instance.SetData("127.0.0.1", 8080);
        ClientMultiplayerManager.Instance.StartPeer(); 
        // a bit weary here..
        if(ArenaManager.Instance.GetCurrentArena() == null)
        {
            ArenaManager.Instance.LoadArena(_arenas[_arenaListOptions.Selected].Id);
        }
        UIManager.Instance.TrySetUIState(UIManager.UIState.HUD);
        Multiplayer.ConnectedToServer += () => 
        {
            GameManager.Instance.StartGame(false);
        };
    }
}