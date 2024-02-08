using Godot;

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
        MD.Log(MD.Runtime.GAME, "ArenaManager", "id to load is :" + id);
        var arena = DataManager.Instance.GetArena(id);
        if(arena != null)
        {
            MD.Log(MD.Runtime.GAME, "ArenaManager.cs", "Loading Arena...");
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
            MD.Log("No Arena to unload..");
        }
    }
}