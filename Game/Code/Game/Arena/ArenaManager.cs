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
        ArenaContainer = GetNode<Arena>("%ArenaContainer");
        base._Ready();
    }

    public void AddPlayerEntity(PlayerEntity player)
    {
        currentArena.AddPlayerEntity(player);
    }

    public Arena GetCurrentArena()
    {
        return currentArena;
    }

    public bool LoadArena(int id)
    {
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
    }
}