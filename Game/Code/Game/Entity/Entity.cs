using Godot;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.Entity.Components;

namespace Mdmc.Code.Game.Entity;

public partial class Entity : Node3D
{
    public enum TeamType
    {
        Player,
        Friend,
        Foe,
        Neutral
    }
    [ExportGroup("Serialized Properties")]
    [Export]
    private bool targetable = false;
    [Export]
    private TeamType team = TeamType.Neutral;    
    [ExportGroup("Sync Properties")]
    [Export]
    public int TargetId = -1;  
    [Export]
    public string EntityName = "Unnamed Entity";
    [Export]
    public float EntityHeight = 2f;

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
        set { targetable = value; }
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
    public Entity CurrentTarget { get; private set; }
    #endregion

    public override void _Ready()
    {
        controller = (EntityController)GetNode("%Controller");
        modifiers = GetNode<EntityModifiers>("%EntityModifiers");
        status = GetNodeOrNull<EntityStatus>("%EntityStatus");
    }

    public void ChangeTarget(Entity target)
    {
        CurrentTarget = target;
        Rpc(nameof(SyncTarget), int.Parse(target.Name));
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncTarget(int id)
    {
        var entities = ArenaManager.Instance.GetCurrentArena().GetEntities();
        if(entities.Count > 0)
        {
            var entity = entities.Find(e => int.Parse(e.Name) == id );
            if(entity != null) CurrentTarget = entity;
            return;
        }
        CurrentTarget = null;
    }
}