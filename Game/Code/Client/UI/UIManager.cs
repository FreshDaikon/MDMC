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

    public override void _PhysicsProcess(double delta)
    {
        if(Input.IsActionJustPressed("OpenMenu"))
        {
            TrySetUIState(UIState.Frontend);
        }
        if(Input.IsActionJustPressed("OpenConfig") && _currentState != UIState.Ingame)
        {
            TrySetUIState(UIState.Ingame);
        }        
    }

    public void TrySetUIState(UIState state)
    {
        MD.Log("Got request to change UI state [ Change to :" + state.ToString());
        if(state == _currentState)
        {
            MD.Log("State was already the active one - skip!");
            //Don't do anything - return some error.
            return;
        }
        else
        {
            MD.Log("Set new state:");
            _currentState = state;
            if(UIContainer.GetChildCount() > 0)
            {
                MD.Log("Remove old UI..");
                foreach(var ui in UIContainer.GetChildren())
                {
                    ui.QueueFree();
                }
            }
            switch(state)
            {
                case UIState.Ingame:
                    MD.Log("Add Ingame Menu..");
                    var ingame = ingamemenuScene.Instantiate();
                    UIContainer.AddChild(ingame);
                    break;
                case UIState.Frontend:
                    MD.Log("Add Frontend..");
                    var frontend = frontendScene.Instantiate(); 
                    UIContainer.AddChild(frontend);
                    break;
                case UIState.HUD:
                    MD.Log("Add HUD..");
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