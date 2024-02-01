using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Godot;

/// <summary>
/// The basis for Manipulating Entities spatially.<br/>
/// It can and should only handle those things.<br/>
/// *SERVER ONLY* <br/>
/// </summary>
public partial class EntityController : CharacterBody3D
{
    public List<Vector3> Forces = new List<Vector3>();
    // Gravity Should Apply to all Entities:
    private float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    [Export]
    private MultiplayerSynchronizer synchronizer;
    private ulong lastUpdate = 0;
    [Export]
    public Vector3 SyncPosition; 
    [Export]
    public Vector3 SyncRotation;


    public override void _PhysicsProcess(double delta)
    {
        if(Multiplayer.GetUniqueId() == 1)
        {
            Vector3 velocity = Vector3.Zero;
            //Add up all the forces:
            foreach(var force in Forces)
            {
                velocity += force;
            }
            Velocity = velocity;
            MoveAndSlide();
            Forces.Clear();
            SyncPosition = Position;
            SyncRotation = Rotation;
        }        
        else
        {
            UpdateController();
        }
    }
    private void UpdateController()
    {  
        Position = Position.Lerp(SyncPosition, 0.5f);
        Rotation = Rotation.Lerp(SyncRotation, 0.5f);
    }
    private bool HasAuth()
    {
        return Multiplayer.GetUniqueId() == 1;
    }
    public void Move(Vector3 force)
    {
        if(!HasAuth())
            return;
        Forces.Add(force);
    }
    public void Rotate(Vector2 direction)
    {
        if(!HasAuth())
            return;
        Rotation = new Vector3(0f, direction.Angle(), 0f);
    }
    public void Teleport(Vector3 destination)
    {
        if(!HasAuth())
            return;
        Position = destination;
    }

}