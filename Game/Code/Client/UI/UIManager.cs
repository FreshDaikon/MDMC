using Godot;
using Mdmc.Code.Game;

namespace Mdmc.Code.Client.UI;

public partial class UIManager : Node
{
    // Static Accessor:
    public static UIManager Instance;
    // Enums:
    public enum UIState
    {
        None,
        Frontend,
        Ingame,
        HUD,
    }
    // Exports:
    [Export] private PackedScene _frontendScene;
    [Export] private PackedScene _hudScene;
    [Export] private PackedScene _ingamemenuScene;
    [Export] private Node _uiContainer;
    
    // Internals:
    private UIState _currentState = UIState.None;    
    private Frontend.LandingPage _landingPage;
    private HUD.Hud _hud;
    private Ingame.IngameMenu _ingame;

    public override void _Ready()
    {
        if(Instance != null)
        {
            QueueFree();
            return; 
        }
        Instance = this;

        _landingPage = _frontendScene.Instantiate<Frontend.LandingPage>();
        _hud = _hudScene.Instantiate<HUD.Hud>();
        _ingame = _ingamemenuScene.Instantiate<Ingame.IngameMenu>();
        _landingPage.Visible = false;
        _hud.Visible = false;
        _ingame.Visible = false;
        _landingPage.ProcessMode = ProcessModeEnum.Disabled;
        _hud.ProcessMode = ProcessModeEnum.Disabled;
        _ingame.ProcessMode = ProcessModeEnum.Disabled;
        _uiContainer.AddChild(_landingPage);
        _uiContainer.AddChild(_hud);
        _uiContainer.AddChild(_ingame);
    }

    public override void _ExitTree()
    {
		if(Instance == this )
		{
			Instance = null;
		}
        base._ExitTree();
    }

    public override void _PhysicsProcess(double delta)
    {
        if(Input.IsActionJustPressed("Start"))
        {
            if(GameManager.Instance == null) return;
            
            if (GameManager.Instance.IsGameRunning())
            {
                if(_currentState == UIState.HUD)
                {
                    TrySetUIState(UIState.Frontend);
                }
                else
                {
                    TrySetUIState(UIState.HUD);
                }
            }
        }
        if(Input.IsActionJustPressed("Select"))
        {
            if(GameManager.Instance == null) return;

            if (GameManager.Instance.IsGameRunning())
            {
                if (_currentState == UIState.HUD)
                {
                    TrySetUIState(UIState.Ingame);
                }
                else
                {
                    TrySetUIState(UIState.HUD);
                }
            }
        }        
    }

    public UIState GetCurrentState()
    {
        return _currentState;
    }

    public void TrySetUIState(UIState state)
    {
        GD.Print("Got request to change UI state [ Change to :" + state.ToString());
        if(state == _currentState)
        {
            GD.Print("State was already the active one - skip!");
            //Don't do anything - return some error.
            return;
        }
        else
        {
            GD.Print("Set new state:");
            _currentState = state;
            if(_uiContainer.GetChildCount() > 0)
            {
                GD.Print("Remove old UI..");
                foreach(var ui in _uiContainer.GetChildren())
                {
                    var control = (Control)ui;
                    control.ProcessMode = ProcessModeEnum.Disabled;
                    control.Visible = false;
                }
            }
            switch(state)
            {
                case UIState.Ingame:
                    GD.Print("Add Ingame Menu..");
                    _ingame.ProcessMode = ProcessModeEnum.Always;
                    _ingame.Visible = true;
                    break;
                case UIState.Frontend:
                    GD.Print("Add Frontend..");
                    _landingPage.ProcessMode = ProcessModeEnum.Always;
                    _landingPage.Visible = true;
                    break;
                case UIState.HUD:
                    GD.Print("Add HUD..");
                    _hud.ProcessMode = ProcessModeEnum.Always;
                    _hud.Visible = true;
                    break;
                case UIState.None:
                    //Screen should clear.
                    break;
                default:
                    break;
            }
        }
    }
}