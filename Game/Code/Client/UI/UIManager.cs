using Daikon.Helpers;
using Godot;

namespace Daikon.Client;

public partial class UIManager : Node
{
    public static UIManager Instance;

    public enum UIState
    {
        None,
        Frontend,
        Ingame,
        HUD,
    }
    [Export]
    private PackedScene frontendScene;
    [Export]
    private PackedScene hudScene;
    [Export]
    private PackedScene ingamemenuScene; 
    private UIState _currentState = UIState.None;    
    private Node UIContainer;

    public override void _Ready()
    {
        if(Instance != null)
        {
            QueueFree();
            return; 
        }
        Instance = this;
        UIContainer = GetNode("%UIContainer");
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
            if(_currentState == UIState.HUD)
            {
                TrySetUIState(UIState.Frontend);
            }
            else
            {
                TrySetUIState(UIState.HUD);
            }
        }
        if(Input.IsActionJustPressed("Select"))
        {
            if(_currentState == UIState.HUD)
            {
                TrySetUIState(UIState.Ingame);
            }
            else
            {
                TrySetUIState(UIState.HUD);
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
            if(UIContainer.GetChildCount() > 0)
            {
                GD.Print("Remove old UI..");
                foreach(var ui in UIContainer.GetChildren())
                {
                    ui.QueueFree();
                }
            }
            switch(state)
            {
                case UIState.Ingame:
                    GD.Print("Add Ingame Menu..");
                    var ingame = ingamemenuScene.Instantiate();
                    UIContainer.AddChild(ingame);
                    break;
                case UIState.Frontend:
                    GD.Print("Add Frontend..");
                    var frontend = frontendScene.Instantiate(); 
                    UIContainer.AddChild(frontend);
                    break;
                case UIState.HUD:
                    GD.Print("Add HUD..");
                    var hud = hudScene.Instantiate();
                    UIContainer.AddChild(hud);
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