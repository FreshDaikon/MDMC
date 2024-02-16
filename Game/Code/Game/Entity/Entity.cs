using Godot;
 
namespace Daikon.Game;

public partial class Entity : Node3D
{
    public enum TeamType
    {
        Friend,
        Foe,
        Neutral
    }
    [ExportGroup("Serialized Properties")]
    [Export]
    private bool targetable = false;
    [Export]
    private TeamType team = TeamType.Neutral;    
    
    /// <summary>
    /// Target ID System:
    /// </summary>
    [ExportGroup("Sync Properties")]
    [Export]
    public int TargetId = -1;
  
    [Export]
    public string EntityName = "Unamed Entity";

    // Internal Data:
    private EntityController controller;
    private EntityModifiers modifiers;
    private EntityStatus status;
    // Getters :
    #region Getters
    //Getters:
    public TeamType Team 
    {
        get { return team; }
    }
    public bool Targetable
    {
        get { return targetable; }
    }
    public EntityController Controller
    {
        get { return controller; }
    }
    public EntityStatus Status
    {
        get { return status; }
    }
    public EntityModifiers Modifiers
    {
        get { return modifiers; }
    }  
    public Entity CurrentTarget { 
        get {
            if(TargetId != -1)
            {
                var target = ArenaManager.Instance.GetCurrentArena().GetEntity(TargetId);
                if(target == null)
                { 
                    TargetId = -1;    
                    return null;            
                }
                else 
                {
                    return target;
                }
            }
            else
            {
                return null;
            } 
        }
    }
    #endregion

    public override void _Ready()
    {
        controller = (EntityController)GetNode("%Controller");
        modifiers = GetNode<EntityModifiers>("%EntityModifiers");
        status = GetNodeOrNull<EntityStatus>("%EntityStatus");
        base._Ready();
    }
}