using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Mdmc.Code.Game.Combat.ModifierSystem;

[GlobalClass]
public partial class TickController : Node 
{
    [Export] private ModifierHandler _modifier;
    [Export] private float _tickRate;
       
    private List<ModifierTick> _modifierTicks = new();
    
    // Time Keeping:
    private double _startTime; 
    private double _lastLapse = 0;
    private int _ticks = 0;
    
    public override void _Ready()
    {
        _modifierTicks = GetChildren().Where(a => a is ModifierTick).Cast<ModifierTick>().ToList();
        _startTime = _modifier.StartTime;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(Multiplayer.IsServer())
        {   
            double lapsed = GameManager.Instance.GameClock - _startTime;    
            double scaled = lapsed * _tickRate;
            if((int)scaled > _lastLapse)
            {
                _ticks += 1;
                _lastLapse = (int)scaled;

                for(int s = 0; s < _modifier.Stacks; s++)
                {
                    foreach(var tick in _modifierTicks)
                    {
                        tick.Tick();
                    }
                }                                   
            }                   
        }        
    }   
}