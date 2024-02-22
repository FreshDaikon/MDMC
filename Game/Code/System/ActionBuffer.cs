using Godot;
using System.Collections.Generic;

namespace Daikon.Helpers;

public partial class ActionBuffer : Node
{
    [Export]
    public int BufferWindow = 1000;
    public static ActionBuffer Instance { get; private set; }

    private Dictionary<string, int> _actionStamps;

    public override void _Ready()
    {
        if(Instance != null)
        {
            GD.PrintErr("ActionBuffer already exists!");
            Free();
            return;
        }
        Instance = this;
        _actionStamps = new Dictionary<string, int>();
    }

    public override void _Process(double delta)
    {        
        foreach(var action in _actionStamps)
        {
            if(Input.IsActionPressed(action.Key))
            {
                //GD.Print("update action stamp: [ " + action.Key + " ] @ :" + Time.GetTicksMsec());
                _actionStamps[action.Key] = (int)Time.GetTicksMsec();
            }
        }
    }

    public bool IsActionPressedBuffered(string action)
    {
        if(_actionStamps.ContainsKey(action))
        {
            var stamp = _actionStamps[action];
            GD.Print(action + " Action was stamped at :" + stamp);
            GD.Print("Current Time is:" + Time.GetTicksMsec());
            GD.Print((int)Time.GetTicksMsec() - stamp);
            if((int)Time.GetTicksMsec() - stamp <= BufferWindow)
            {
                _actionStamps[action] = (int)Time.GetTicksMsec();
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    } 

    public void RegisterAction(string action)
    {
        _actionStamps.Add(action, 0);
    }
}

