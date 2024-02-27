using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class ArenaManager : Node
{  

    public static ArenaManager Instance;
   
    private Node3D ArenaContainer;
    private Arena currentArena;

    public override void _Ready()
    {
        if(Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        ArenaContainer = GetNode<Node3D>("%ArenaContainer");
        base._Ready();
    }

    public Arena GetCurrentArena()
    {
        return currentArena;
    }

    public Node3D GetRealizationPool()
    {
        if(currentArena != null)
        {
            return currentArena.RealizationPool;
        }
        return null;
    }

    public bool HasArena()
    {
        return currentArena != null;
    }

    public bool LoadArena(int id)
    {
        GD.Print("ArenaManager id to load is : " + id);
        var arena = DataManager.Instance.GetArenaInstance(id);
        if(arena != null)
        {
            ArenaContainer.AddChild(arena);
            currentArena = arena;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void UnloadArena()
    {
        if(currentArena != null)
        {
            ArenaContainer.RemoveChild(currentArena);
            currentArena.QueueFree();
        }
        else
        {
            GD.Print("No Arena to unload..");
        }
    }
}