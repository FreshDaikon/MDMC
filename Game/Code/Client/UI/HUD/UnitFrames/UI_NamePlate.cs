using Daikon.Game;
using Godot;

namespace Daikon.Client;

public partial class UI_NamePlate : Control
{
    [Export]
	private Vector2 BarSize = new Vector2(150, 15f);
    [Export]
    private float ChangeSpeed = 60f;

    private Vector2 _uiPosition;
	private Camera3D _camera;
	private Vector3 _worldPosition;

    private Entity _entity;

	private TextureRect HealthBar;
	private Label NameLabel;

    public override void _Ready()
    {
        HealthBar = GetNode<TextureRect>("%HealthBar");
		NameLabel = GetNode<Label>("%Name");
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
        if(_entity == null)
        {
            CallDeferred(nameof(QueueFree));
        }
        _camera = GetViewport().GetCamera3D();
        var entityPos = _entity.Controller.GlobalPosition;
        Visible = !_camera.IsPositionBehind(entityPos) && _entity.Status.CurrentState != EntityStatus.StatusState.KnockedOut;
        NameLabel.Text = _entity.EntityName;
        _uiPosition = _camera.UnprojectPosition(entityPos + new Vector3(0f, _entity.EntityHeight, 0f));
		Position = _uiPosition; 
        var currentHealth = (float)_entity.Status.CurrentHealth / (float)_entity.Status.MaxHealth;
        HealthBar.Visible = currentHealth > 0f;
        HealthBar.Size = new Vector2(BarSize.X * currentHealth, BarSize.Y);
    }
}