using Godot;
using Godot.Collections;

namespace Mdmc.Code.System;

public partial class InputBuffer : Node
{
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
        switch (@event)
        {
            case InputEventKey keyEvent when !keyEvent.Pressed || keyEvent.IsEcho():
                return;
            case InputEventKey keyEvent:
            {
                var key = keyEvent.AsTextKeycode();
                if(!_keyboardStamps.ContainsKey(key))
                {
                    _keyboardStamps.Add(key, Time.GetTicksMsec());
                }
                else
                {
                    _keyboardStamps[key] = Time.GetTicksMsec();
                }
                
                break;
            }
            case InputEventJoypadButton button:
            {
                if(!button.Pressed || button.IsEcho()) return;

                var index = button.ButtonIndex.ToString();
                if(!_joyStamps.ContainsKey(index))
                {
                    _joyStamps.Add(index, Time.GetTicksMsec());
                }
                else
                {
                    _joyStamps[index] = Time.GetTicksMsec();
                }

                break;
            }
        }
    }

    public bool IsActionPressedBuffered(string action, float bufferWindow = 1000)
    {
        foreach(var @event in InputMap.ActionGetEvents(action))
        {
            switch (@event)
            {
                case InputEventKey keyEvent when _keyboardStamps.ContainsKey(keyEvent.AsTextKeycode()):
                {
                    var stamp = _keyboardStamps[keyEvent.AsTextKeycode()];
                    if(Time.GetTicksMsec() - stamp <= bufferWindow)
                    {
                        InvalidateAction(action);
                        return true;
                    }

                    break;
                }
                case InputEventJoypadButton joyEvent when _joyStamps.ContainsKey(joyEvent.ButtonIndex.ToString()):
                {
                    var stamp = _joyStamps[joyEvent.ButtonIndex.ToString()];
                    if(Time.GetTicksMsec() - stamp <= bufferWindow)
                    {
                        InvalidateAction(action);
                        return true;
                    }

                    break;
                }
            }
        }
        return false;
    }

    private void InvalidateAction(string action)
    {
        foreach(var @event in InputMap.ActionGetEvents(action))
        {
            switch (@event)
            {
                case InputEventKey key:
                {
                    if(_keyboardStamps.ContainsKey(key.AsTextKeycode()))
                    {
                        _keyboardStamps.Remove(key.AsTextKeycode());
                    }

                    break;
                }
                case InputEventJoypadButton button:
                {
                    if(_joyStamps.ContainsKey(button.ButtonIndex.ToString()))
                    {
                        _joyStamps.Remove(button.ButtonIndex.ToString());
                    }

                    break;
                }
            }
        }
    }
}