using Godot;
using Godot.Collections;

namespace Daikon.Helpers;

public partial class InputBuffer : Node
{
    [Export]
    public float BufferWindow = 200f;

    public static InputBuffer Instance { get; private set; }
    private Dictionary<string, ulong> _keyboardStamps;
    private Dictionary<string, ulong> _joyStamps;

    public override void _Ready()
    {
        if(Instance != null)
        {
            GD.PrintErr("InputBuffer already exists!");
            Free();
            return;
        }
        Instance = this;
        _keyboardStamps = new Dictionary<string, ulong>();
        _joyStamps = new Dictionary<string, ulong>();

        base._Ready();
    }

    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventKey)
        {
            InputEventKey keyEvent = (InputEventKey)@event;
            if(!keyEvent.Pressed || keyEvent.IsEcho())
            {
               return; 
            } 
            var key = keyEvent.AsTextKeycode();
            if(!_keyboardStamps.ContainsKey(key))
            {
                _keyboardStamps.Add(key, Time.GetTicksMsec());
            }
            else
            {
                _keyboardStamps[key] = Time.GetTicksMsec();
            }

        }
        else if(@event is InputEventJoypadButton)
        {
            InputEventJoypadButton joyEvent = (InputEventJoypadButton)@event;
            if(!joyEvent.Pressed || joyEvent.IsEcho()) return;

            var index = joyEvent.ButtonIndex.ToString();
            if(!_joyStamps.ContainsKey(index))
            {
                _joyStamps.Add(index, Time.GetTicksMsec());
            }
            else
            {
                _joyStamps[index] = Time.GetTicksMsec();
            }
        }
    }

    public bool IsActionPressedBuffered(string action, float bufferWindow = 200f)
    {
        foreach(InputEvent @event in InputMap.ActionGetEvents(action))
        {            
            if(@event is InputEventKey keyEvent && _keyboardStamps.ContainsKey(keyEvent.AsTextKeycode()))
            {
                ulong stamp = _keyboardStamps[keyEvent.AsTextKeycode()];
                if(Time.GetTicksMsec() - stamp < bufferWindow)
                {
                    InvalidateAction(action);
                    return true;
                }
            }
            else if(@event is InputEventJoypadButton joyEvent && _joyStamps.ContainsKey(joyEvent.ButtonIndex.ToString()))
            {
                ulong stamp = _joyStamps[joyEvent.ButtonIndex.ToString()];
                if(Time.GetTicksMsec() - stamp < bufferWindow)
                {
                    InvalidateAction(action);
                    return true;
                }
            }
        }
        return false;
    }

    private void InvalidateAction(string action)
    {
        foreach(InputEvent @event in InputMap.ActionGetEvents(action))
        {
            if(@event is InputEventKey)
            {
                InputEventKey keyEvent = (InputEventKey)@event;
                if(_keyboardStamps.ContainsKey(keyEvent.AsTextKeycode()))
                {
                    _keyboardStamps.Remove(keyEvent.AsTextKeycode());
                }
            }
            else if(@event is InputEventJoypadButton)
            {
                InputEventJoypadButton joyEvent = (InputEventJoypadButton)@event;
                if(_joyStamps.ContainsKey(joyEvent.ButtonIndex.ToString()))
                {
                    _joyStamps.Remove(joyEvent.ButtonIndex.ToString());
                }
            }
        }
    }
}