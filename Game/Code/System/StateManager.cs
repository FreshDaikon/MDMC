using System.Text.RegularExpressions;
using Godot;

public partial class StateManager : Node
{

    
    public static StateManager Instance;
    public bool HasFocus = true;

    public override void _Ready()
    {
        if(Instance != null)
        {
            this.Free();
        }
        Instance = this;
    }
    public override void _Notification(int what)
    {
        switch(what)
        {
            case (int)NotificationApplicationFocusOut:
                HasFocus = false;
                break;
            case (int)NotificationApplicationFocusIn:
                HasFocus = true;
                break;
        }
    }
}