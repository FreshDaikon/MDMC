using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class Modifier : Node
{
    // Exported Variables:
    [Export]
    public Texture2D Icon;
    [Export]
    public string ModifierName = "";
    [Export]
    public bool IsPermanent = false;
    [Export]
    public float Duration = 5f;
    [Export]
    public bool IsTicked = false;
    [Export(PropertyHint.Range, "1, 3")]
    public float TickRate = 1f;
    [Export]
    public bool CanStack = false;
    [Export]
    public int Stacks = 1;
    [Export]
    public int MaxStacks = 1;
    [Export]
    private double startTime;    
    [Export]
    public double TimeRemaining = -1f;

    [Export]
    public StatMod[] StatMods { get; set; }
    public ModifierObject Data;

    //Time Keeping continued:
    private int lastLapse = 0;
    private int ticks = 0;

    // Entity to which this mod is attached:
    public EntityStatus targetStatus;
    private NodePath ModActionsPath;
    private NodePath ModModsPath;

    //Entity who applied this modifier:
    

    public override void _Ready()
    {
        if(!IsPermanent)
        {
            startTime = GameManager.Instance.GameClock;            
        }          
        if(Multiplayer.GetUniqueId() == 1)
        {
            foreach(var mod in StatMods)
            {
                targetStatus.AddStatMod(mod);
            }
        }
        base._Ready();
    }
    public override void _PhysicsProcess(double delta)
    {
        if(Multiplayer.GetUniqueId() == 1)
        {   
            if(IsPermanent)
                return;
            double lapsed = (GameManager.Instance.GameClock - startTime);       
            TimeRemaining = Duration - lapsed;     
            if(IsTicked)
            {
                double scaled = lapsed * TickRate;
                if((int)scaled > lastLapse)
                {
                    ticks += 1;
                    lastLapse = (int)scaled;
                    Tick();                    
                }
            }            
            if(lapsed > Duration)
            {
                MD.Log(" Total Ticks : " + ticks );
                QueueFree();
            }
        }        
        else
        {
            //This is just for Client Side Stuff:
            double lapsed = GameManager.Instance.GameClock - startTime;            
        }
    }
    public override void _ExitTree()
    {
        if(Multiplayer.GetUniqueId() == 1)
        {
            RemoveStatMods();
        }
        base._ExitTree();
    }
    
    public double GetTimeRemaining()
    {
        if(IsPermanent)
        {
            return 1f;
        }
        else
        {
            var lapsed = GameManager.Instance.GameClock - startTime;
            var remaining = Mathf.Clamp(Duration - lapsed, 0, Duration);
            return remaining / Duration;
        }
    }
    public void RemoveStatMods()
    {
        foreach(var mod in StatMods)
        {
            MD.Log("Removing Mod : " + mod.Name);
            targetStatus.RemoveStatMod(mod);
        }
    }
    public virtual void Tick()
    {
        
    }
}