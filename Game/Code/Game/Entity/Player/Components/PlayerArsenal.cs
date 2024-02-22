using System.Linq;
using Godot;
using Daikon.Helpers;

namespace Daikon.Game;

public partial class PlayerArsenal: Node
{
    public static class ContainerNames
    {
        public static string Main { get { return "Main"; }}
        public static string Left { get { return "Left"; }}
        public static string Right { get { return "Right"; }}
    }
    [Export(PropertyHint.Dir)]
    private string skillContainersPath;    
    private Node skillContainers ;
    private MultiplayerSpawner spawner;
    public PlayerEntity Player;

    // Casting Control : Remember to Sync IsCasting and Castime
    //
    // Unfortunately these have to be exposed to be synced.
    [ExportGroup("Sync Properties")]
    [Export]
    public bool IsCasting = false;
    [Export]
    public ulong CastingStartTime;
    [Export]
    public ulong CastingTime;
    [Export]
    public bool IsChanneling = false;
    [Export]
    public ulong ChannelingStartTime;
    [Export]
    public ulong ChannelingTime;
    // Later we can export this as a ID as well.
    public Skill CastingSkill;
    public Skill ChannelingSkill;

    public float GCD = 2.5f;
    public ulong GCDStartTime = 0;

    public override void _Ready()
    {
        spawner = GetNode<MultiplayerSpawner>("%Spawner");
        skillContainers = GetNode("%SkillContainers");
        Player = GetParent<PlayerEntity>();
        GetArsenalPaths();
        if(Multiplayer.GetUniqueId() == 1)
        {
            MD.Log("Since we are the Server, we can add the containers here");
            CallDeferred(nameof(DEBUG_InitialSetup));    
        }       
        // To Start Reset Style.
        base._Ready();
    }
    public void ResetArsenal()
    {
        var modGCD = Player.Status.GetGCDModifier();
        GCDStartTime = GameManager.Instance.ServerTick - (ulong)(modGCD * 1000f);
        Rpc(nameof(SyncStartTime), GCDStartTime);
        IsCasting = false;
        IsChanneling = false;
        CastingSkill = null;
        ChannelingSkill = null;

        //Reset Skills:
        if(skillContainers.GetChildCount() > 0)
        {
            var containers = skillContainers.GetChildren()
                .Where(c => c is SkillContainer)
                .Cast<SkillContainer>()
                .ToList();

            foreach(var container in containers)
            {
                container.ResetContainer();
            }
        }
    }

    private void DEBUG_InitialSetup()
    {
        MD.Log("DEBUG_InitialSetup");
        SetSkillContainer(-525528611, ContainerNames.Main);
        SetSkillContainer(-557544443, ContainerNames.Left);
        SetSkillContainer(462203807, ContainerNames.Right);
        SetSkill(1156137064, ContainerNames.Main, 0);        
        SetSkill(1156137064, ContainerNames.Main, 1);        
        SetSkill(1156137064, ContainerNames.Main, 2);        
        SetSkill(1156137064, ContainerNames.Main, 3);   
        SetSkill(1156137064, ContainerNames.Left, 0);        
        SetSkill(1156137064, ContainerNames.Left, 1);        
        SetSkill(1156137064, ContainerNames.Left, 2);        
        SetSkill(1156137064, ContainerNames.Left, 3); 
        SetSkill(1156137064, ContainerNames.Right, 0);        
        SetSkill(1156137064, ContainerNames.Right, 1);        
        SetSkill(1156137064, ContainerNames.Right, 2);        
        SetSkill(1156137064, ContainerNames.Right, 3); 
    }

    public void SetSkillContainer(int id, string containerName)
    {
        var newContainer = DataManager.Instance.GetSkillContainer(id);
        if(newContainer == null)
        {
            MD.Log("Skill Container with ID: " + id + " does not exist.");
            return;
        }
        // If getting by ID returned a usable object
        // First check if one such container already exists.
        // if it does, remove it and...
        if(skillContainers.GetChildCount() > 0)
        {
            var containers = skillContainers.GetChildren()
                .Where(c => c is SkillContainer)
                .Cast<SkillContainer>()
                .ToList();

            foreach(var container in containers)
            {
                if(container.Name == containerName)
                {
                    MD.Log("Removing container: " + containerName);
                    container.QueueFree();
                }
            }
        }
        // Replace it with the new one:
        MD.Log("Adding container: " + containerName);
        newContainer.Name = containerName;
        skillContainers.AddChild(newContainer);
    }
    // Setup the Arsenal's Spawner to have access to all the SkillContainers
    private void GetArsenalPaths()
    {
        using var dir = DirAccess.Open(skillContainersPath);
        if(dir != null)
        {
            dir.ListDirBegin();
            string file = dir.GetNext();
            while(file != "")
            {
                spawner.AddSpawnableScene(skillContainersPath + "/" + file.Replace(".remap", ""));
                file = dir.GetNext();
            }
            dir.ListDirEnd();
        }     
    }

    public void StartCasting(Skill caster)
    {
        IsCasting = true;
        CastingStartTime = GameManager.Instance.ServerTick;
        CastingTime = (ulong)(caster.CastTime * 1000f);
        CastingSkill = caster;
    }
    public void StartChanneling(Skill caster)
    {
        IsChanneling = true;
        ChannelingStartTime = GameManager.Instance.ServerTick;
        ChannelingTime = (ulong)(caster.ChannelTime * 1000f);
        ChannelingSkill = caster;
    }
    public void FinishCasting(SkillResult result)
    {
        MD.Log("Skill result after casting:" + result.result);
        CastingSkill = null;
        IsCasting = false;
    }
    public void FinishChanneling(SkillResult result)
    {
        MD.Log("Skill result after channeling:" + result.result);
        ChannelingSkill.InteruptChannel();
        ChannelingSkill = null;
        IsChanneling = false;
    }
    public void TryInteruptCast()
    {
        if(IsCasting && !CastingSkill.CanMove)
        {
            CastingSkill.InteruptCast();
            FinishCasting(new SkillResult() { result = MD.ActionResult.CASTING_INTERUPTED } );            
        }
    }
    public void TryInteruptChanneling()
    {
        if(IsChanneling && !ChannelingSkill.CanMove)
        {
            ChannelingSkill.InteruptCast();
            FinishChanneling(new SkillResult(){  } );
        }
    }

    public void SetSkill(int id, string containerName, int slot)
    {
        var container = GetSkillContainer(containerName);
        if(container != null)
        {
            container.SetSkill(id, slot);
        }
    }
    public Skill GetSkill(string containerName, int slot)
    {
        var container = GetSkillContainer(containerName);
        if(container != null)
        {
            return container.GetSkill(slot);
        }
        return null;
    }

    //For Client:
    public SkillResult CanCast(string containerName, int index)
    {
        var result = new SkillResult() { SUCCESS = false, result= MD.ActionResult.ERROR };
        //Check Arsenal:
        if(IsCasting)
        {
            //MD.Log("Return because we are casting");
            result.result = MD.ActionResult.IS_CASTING;
            return result;
        }
        if(IsChanneling)
        {
            //MD.Log("Return because we are channeling");
            result.result = MD.ActionResult.IS_CHANNELING;
            return result;
        }
        if(IsArsenalOnCD())
        {
            //MD.Log("Return because we are on Arsenal CD");
            result.result = MD.ActionResult.ON_COOLDOWN;
            return result;
        }  
        // Try Container:
        var container = GetSkillContainer(containerName);
        if(container == null)
        {
            //MD.Log("Return because container is null");
            result.result = MD.ActionResult.ERROR;
            return result;
        }
        //Finally Try Skill (TODO implement OGCD and Cooldown)
        var skill = container.GetSkill(index);
        if(skill == null)
        {
            //MD.Log("Return because skill is null");
            result.result = MD.ActionResult.ERROR;
            return result;
        }
        if(skill.IsOnCooldown())
        {            
            //MD.Log("Return because skill is on CD");
            //MD.Log("Skill Start Time:" + skill.StartTime);
            //MD.Log("Server Time :" + GameManager.Instance.ServerTick);
            result.result = MD.ActionResult.ON_COOLDOWN;
            return result;
        }
        var quickCheck = skill.CheckSkill();
        if(!quickCheck.SUCCESS)
        {
            return result;
        }
        //If all else is ok, send back the good to go:
        //MD.Log("Return because we are good to go");
        result.SUCCESS = true;
        result.result = MD.ActionResult.CAST;
        return result;
    }
    public SkillContainer GetSkillContainer(string containerName)
    {
        if(skillContainers.GetChildCount() > 0)
        {
            var containers = skillContainers.GetChildren()
                .Where(c => c is SkillContainer)
                .Cast<SkillContainer>()
                .ToList();

            foreach(var container in containers)
            {
                if(container.Name == containerName)
                {
                    return container;
                }
            }
        }
        return null;
    }

    public bool IsArsenalOnCD()
    {
        var lapsed = (GameManager.Instance.ServerTick - GCDStartTime) / 1000f;
        var modGCD = Player.Status.GetGCDModifier();
        if(lapsed < modGCD)
        {
            return true;
        }
        return false;
    }

    public float GetArsenalGCD()
    {
        var modGCD = Player.Status.GetGCDModifier();
        return modGCD;
    }
    // Active Systems:
    public SkillResult TriggerSkill(string containerName, int index)
    {
 
         var container = GetSkillContainer(containerName);
        if(container != null)
        {
            if(IsCasting)
            {
                return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.IS_CASTING };
            }
            if(IsChanneling)
            {
                return container.TriggerSkill(index); 
            }
            if(container.IsSkillOGCD(index))
            {
                MD.Log("Try Trigger OGCD Skill:");
                return container.TriggerSkill(index);
            }
            
            var lapsed = (GameManager.Instance.ServerTick - GCDStartTime) / 1000f;
            var modGCD = Player.Status.GetGCDModifier(); 
            MD.Log("Lapsed: " + lapsed + " GCD: " + modGCD);
            if(lapsed < modGCD)
            {
                return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.ON_COOLDOWN };
            }
            else
            {
                var result = container.TriggerSkill(index);
                if(result.SUCCESS)
                {
                    MD.Log("We triggered and it was success.");
                    Rpc(nameof(SyncStartTime), GameManager.Instance.ServerTick);
                    GCDStartTime = GameManager.Instance.ServerTick;
                    return result;
                }
                else
                {
                    MD.Log("We triggered, but it was not success");
                    return result;
                }
            }      
        }
        return new SkillResult(){ SUCCESS = false, result = MD.ActionResult.ERROR };
    }          

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SyncStartTime(ulong time)
    {
        MD.Log("Syncing Arsenal GCD Start Time to: " + time + ", Current time is : " + GameManager.Instance.ServerTick );
        GCDStartTime = time;
    }
    public float[] GetArsenalSkillWeights()
    {
        float[] weights = new float[3];
        if(skillContainers.GetChildCount() > 0)
        {
            var containers = skillContainers.GetChildren()
                .Where(c => c is SkillContainer)
                .Cast<SkillContainer>()
                .ToList();

            foreach(var container in containers)
            {                
                var skillSlots = container.GetSkillSlots();
                foreach(var slot in skillSlots)
                {
                    var skill = slot.GetSkill();
                    if(skill == null || skill.IsUniversalSkill)
                    {
                        weights[0] += 0.333f;
                        weights[1] += 0.333f;
                        weights[2] += 0.333f;
                        continue;
                    }
                    switch(skill.SkillType)
                    {
                        case MD.SkillType.DPS:
                            weights[0] += 1f;
                            break;  
                        case MD.SkillType.HEAL:
                            weights[1] += 1f;
                            break;                                              
                        case MD.SkillType.TANK:
                            weights[2] += 1f;
                            break;   
                    }
                }                
            }       
            return weights;
        }
        else
        {
            return weights;
        }
    } 
    public float GetWeightedTotal(float[] weights)
    {
        var indexValue = weights.ToList().IndexOf(weights.Max()) * 12.0f;
        var indexWeight = weights.Max();
        return indexValue + Mathf.Remap( indexWeight, 3f, 12f, 12f, 3f);
    }
}