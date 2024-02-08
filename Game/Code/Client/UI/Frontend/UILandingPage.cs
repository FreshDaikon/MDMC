

using System.Collections.Generic;
using Godot;

public partial class UILandingPage : Control
{
    //Status
    private Label WSConnectionLabel;
    private Label GameStatus;
    //Join Code Field
    private LineEdit JoinCodeEdit;
    // Buttons 
    private OptionButton ArenaListOptions;
    private Button RequestGameButton;
    private Button JoinGameButton;
    private Button StartServerButton;
    private Button HostHubButton;

    private List<Arena> _arenas;


    public override void _Ready()
    {
        WSConnectionLabel = GetNode<Label>("%GameStatusLabel");
        ArenaListOptions = GetNode<OptionButton>("%ArenaListOptions");
        JoinCodeEdit = GetNode<LineEdit>("%JoinCodeEdit");
        RequestGameButton = GetNode<Button>("%RequestGameButton");
        JoinGameButton = GetNode<Button>("%JoinGameButton");

        CallDeferred(nameof(GetArenaList));

        //Setup Signals:
        RequestGameButton.Pressed += () => RequestGame();

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

         
    }

    private void GetArenaList()
    {
        _arenas = DataManager.Instance.GetArenas();
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

    private void StartLocalServer()
    {
        ClientMultiplayerManager.Instance.StartLocalServer(_arenas[ArenaListOptions.Selected]);
    }

}