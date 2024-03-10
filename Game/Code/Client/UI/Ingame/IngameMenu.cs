using Godot;
using Mdmc.Code.Client.UI.Ingame.Elements;
using Mdmc.Code.Game;
using Mdmc.Code.Game.Arena;
using Mdmc.Code.Game.Entity.Player;

namespace Mdmc.Code.Client.UI.Ingame;

public partial class IngameMenu: Control
{
    // Exports:
    [Export] private PackedScene _containerButtonScene;
    [Export] private PackedScene _skillButtonScene;
    [Export] private ContainerDisplayItem _mainContainer;
    [Export] private ContainerDisplayItem _leftContainer;
    [Export] private ContainerDisplayItem _rightContainer;
    [Export] private Control _itemSelector;
    [Export] private Control _itemContainer;

    // Internal:
    private bool _itemSelected = false;
    private bool _isContainer = true;
    private PlayerEntity _player;

    public override void _Ready()
    {
        _mainContainer.ContainerSelected += OnContainerSelected;
        _rightContainer.ContainerSelected += OnContainerSelected;
        _leftContainer.ContainerSelected += OnContainerSelected;

        _mainContainer.SlotSelected += OnSlotSelected;
        _rightContainer.SlotSelected += OnSlotSelected;
        _leftContainer.SlotSelected += OnSlotSelected;

    }

    private void OnContainerSelected(int slot)
    {
        _itemSelected = true;
        
        CleanUp();

        var containers = DataManager.Instance.GetAllSkillContainers();
        foreach (var container in containers)
        {
            var newButton = (ContainerButton)_containerButtonScene.Instantiate();
            newButton.SetContainer(container);
            _itemContainer.AddChild(newButton);

            newButton.ContainerPressed += id =>
            {
                GD.Print("Container Selected!");
                _player.Arsenal.TrySetContainer(id, slot);
                _itemSelected = false;
            };
        }

    }

    private void OnSlotSelected(int containerSlot, int skillSlot)
    {
        _itemSelected = true;
        
        CleanUp();

        var skills = DataManager.Instance.GetAllSkills();
        foreach (var skill in skills)
        {
            var newButton = (SkillButton)_skillButtonScene.Instantiate();
            newButton.AssignSkill(skill);
            _itemContainer.AddChild(newButton);
            newButton.SkillPressed += id =>
            {
                GD.Print("SkillSelected " + id + "For Slot:" + containerSlot);
                _player.Arsenal.TrySetSkill(id, containerSlot, skillSlot);
                _itemSelected = false;
            };
        }
    }


    private void CleanUp()
    {
        foreach (var item in _itemContainer.GetChildren())
        {
            item.QueueFree(); 
        }
    }
    
    public override void _Process(double delta)
    {
        if(!GameManager.Instance.IsGameRunning()) return;

        if (_player == null)
        {
            var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
            if(players == null) return;

            _player = players.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());
            
            if(_player == null) return;
        }    
        
        _itemSelector.Visible = _itemSelected;
    }
}