using Godot;
using Mdmc.Code.System;
using Entity = Mdmc.Code.Game.Entity.Entity;
using EntityStatus = Mdmc.Code.Game.Entity.Components.EntityStatus;
using PlayerEntity = Mdmc.Code.Game.Entity.Player.PlayerEntity;

namespace Mdmc.Code.Client.UI.HUD.UnitFrames;

public partial class NamePlate : Control
{
    // Exposed:
    [Export] private Vector2 _barSize = new Vector2(150, 15f);
	[Export] private TextureRect _healthBar;
	[Export] private Label _nameLabel;
    
    // Internals:
    private Vector2 _uiPosition;
	private Camera3D _camera;
	private Vector3 _worldPosition;
    private Entity _entity;

    private GradientTexture1D _barGradient = new();

    public override void _Ready()
    {
        _healthBar.Texture = _barGradient;
    }

    public void InitializeFrame(Entity entity)
    {
        _entity = entity;
    }

    public Entity GetEntity()
    {
        return _entity;
    }

    public override void _PhysicsProcess(double delta)
    {
        switch (_entity)
        {
            case null:
                CallDeferred(nameof(QueueFree));
                break;
            case PlayerEntity entity:
                _barGradient.Gradient = MD.GetPlayerGradient(entity);
                break;
        }
        _camera = GetViewport().GetCamera3D();
        var entityPos = _entity.Controller.GlobalPosition;
        Visible = !_camera.IsPositionBehind(entityPos) && _entity.Status.CurrentState != EntityStatus.StatusState.KnockedOut;
        _nameLabel.Text = _entity.EntityName;
        _uiPosition = _camera.UnprojectPosition(entityPos + new Vector3(0f, _entity.EntityHeight, 0f));
		Position = _uiPosition; 
        var currentHealth = (float)_entity.Status.CurrentHealth / (float)_entity.Status.MaxHealth;
        _healthBar.Visible = currentHealth > 0f;
        _healthBar.Size = new Vector2(_barSize.X * currentHealth, _barSize.Y);
    }
}