using Godot;

/// <summary>
/// A skill slot should exist as a child of a SkillContainer.
/// Should Never stand on its own - needs no ID.
/// </summary>
public partial class SkillSlot : Node
{
    //Serialized Properties:
    [ExportGroup("Skill Slot Properties")]
    [Export]
    public MD.SkillType SlotSkillType { get; set; }
    [Export]
    public float PotencyMultiplier = 1.0f;
    [ExportGroup("System Properties")]
    [Export(PropertyHint.Dir)]
    private string skillsPath;

    //Useful Setters:
    public PlayerEntity Player;

    //Internal Properites:
    private MultiplayerSpawner skillSpawner;
    private Node skillHolder;

    public override void _Ready()
    {
        skillSpawner = GetNode<MultiplayerSpawner>("%Spawner");
        skillHolder = GetNode("%SkillHolder");
        getSkillPaths();
        base._Ready();
    }

    public SkillResult TriggerSkill()
    {
        var skill = skillHolder.GetChildOrNull<Skill>(0);
        if(skill != null)
        {
            return skill.TriggerSkill();
        }
        else
        {
            return new SkillResult()
            {
                SUCCESS = false,
                result = MD.ActionResult.ERROR
            };
        }
    }
    public void ResetSkill()
    {
        var skill = skillHolder.GetChildOrNull<Skill>(0);
        skill?.Reset();
    }
    public Skill GetSkill()
    {
        if(skillHolder.GetChildCount() > 0)
        {
            return (Skill)skillHolder.GetChild(0);
        }
        return null;
    }

    public void SetSkill(int id)
    {
        var newSkill = DataManager.Instance.GetSkill(id);
        if(newSkill == null)
        {
            return;
        }
        // If getting by ID returned a usable object
        // First check if one such container already exists.
        // if it does, remove it and...
        if(skillHolder.GetChildCount() > 0)
        {
            var current = (Skill)skillHolder.GetChild(0);
            current?.Free();
        }
        // Replace it with the new one:
        newSkill.Name = "Skill_" + newSkill.Id;
        newSkill.SkillType = SlotSkillType;
        newSkill.Player = Player == null ? null : Player;
        if(!newSkill.IsUniversalSkill)
        {
            newSkill.AdjustedPotency = (int)(newSkill.BasePotency * PotencyMultiplier);
        }             
        skillHolder.AddChild(newSkill);
    }

    //Get All available skills that we can spawn.
    private void getSkillPaths()
    {
        using var dir = DirAccess.Open(skillsPath);
        if(dir != null)
        {
            dir.ListDirBegin();
            string file = dir.GetNext();
            while(file != "")
            {
                skillSpawner.AddSpawnableScene(skillsPath + "/" + file.Replace(".remap", ""));
                file = dir.GetNext();
            }
            dir.ListDirEnd();
        }
    }
}