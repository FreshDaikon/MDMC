using Godot;

namespace Daikon.Client;

public partial class UIIngameMenu: Control
{
    private Button DisconnectButton;


    public override void _Ready()
    {
        DisconnectButton = GetNode<Button>("%DisconnectButton");
        DisconnectButton.Pressed += () => OnDisconnectButton();
        base._Ready();
    }

    private void OnDisconnectButton()
    {        
        ClientMultiplayerManager.Instance.LeaveServer();
        UIManager.Instance.TrySetUIState(UIManager.UIState.Frontend);
    }



}