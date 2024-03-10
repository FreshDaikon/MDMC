using System.Collections.Generic;
using Godot;
using Mdmc.Code.Client.Connect;
using Mdmc.Code.Game;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.Data;

namespace Mdmc.Code.Client.UI.Frontend;

public partial class LandingPage : Control
{
    // Export :
    [Export] private LineEdit _joinCodeEdit;
    [Export] private OptionButton _arenaListOptions;
    [Export] private Button _requestGameButton;
    [Export] private Button _joinGameButton;
    [Export] private Button _startGameButton;
    [Export] private Button _startServerButton;
    [Export] private Button _joinLocalServerButton;
    [Export] private Button _connectDaikonLocal;
    [Export] private Button _connectDaikonRemote;
    [Export] private Button _getArenaRecords;
    
    // Internal :
    private List<ArenaData> _arenas = new();

    public override void _Ready()
    {
        //Setup Signals:
        _requestGameButton.Pressed += RequestGame;
        _joinGameButton.Pressed += JoinGame;
        _connectDaikonLocal.Pressed += () => ConnectToDaikon(false);
        _connectDaikonRemote.Pressed += () => ConnectToDaikon(true);
        //Local Setup:
        _startServerButton.Pressed += StartLocalServer;
        _joinLocalServerButton.Pressed += JoinLocalServer;

        _startGameButton.ButtonUp += StartGame;
        _getArenaRecords.ButtonUp += GetArenaRecords;
      
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
            Mdmc.Code.Client.Multiplayer.ClientMultiplayerManager.Instance.SetData(serverhost, port);
        };
        DaikonConnect.Instance.GameFound += (host, port) => {
            GD.Print("We have gotten a game here!");
            ArenaManager.Instance.LoadArena(_arenas[_arenaListOptions.Selected].Id);
            Mdmc.Code.Client.Multiplayer.ClientMultiplayerManager.Instance.SetData(host, port);
        };
        
    }

    private void RequestGame()
    {
        DaikonConnect.Instance.DaikonRequestGame(_arenas[_arenaListOptions.Selected].Id);
    }

    private void JoinGame()
    {
        GD.Print("Let's see if we can find a game!");
        DaikonConnect.Instance.DaikonJoinGame(_joinCodeEdit.Text);
    }

    private void GetArenaRecords()
    {
        DaikonConnect.Instance.GetArenaRecords(_arenas[_arenaListOptions.Selected].Id);
    }

    private void StartGame()
    {
        Mdmc.Code.Client.Multiplayer.ClientMultiplayerManager.Instance.StartPeer();
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
        Mdmc.Code.Client.Multiplayer.ClientMultiplayerManager.Instance.StartLocalServer(_arenas[_arenaListOptions.Selected]);
    }
    private void JoinLocalServer()
    {
        Mdmc.Code.Client.Multiplayer.ClientMultiplayerManager.Instance.SetData("127.0.0.1", 8080);
        Mdmc.Code.Client.Multiplayer.ClientMultiplayerManager.Instance.StartPeer(); 
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