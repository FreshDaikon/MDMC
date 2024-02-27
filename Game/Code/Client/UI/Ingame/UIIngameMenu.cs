using Daikon.Game;
using Daikon.Helpers;
using Godot;

namespace Daikon.Client;

public partial class UIIngameMenu: Control
{

    //Imediate Objects:
    private SkillContainer mainContainer;
    private SkillContainer leftContainer;
    private SkillContainer rightContainer;


    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
        if(!GameManager.Instance.IsGameRunning())
            return;
        
        var players = ArenaManager.Instance.GetCurrentArena().GetPlayers();
        if(players == null)
				return;

        var localPlayer = players.Find(p => p.Name == Multiplayer.GetUniqueId().ToString());			
        if(localPlayer != null)
        {
            mainContainer = localPlayer.Arsenal.GetSkillContainer(MD.ContainerSlot.Main);
            leftContainer = localPlayer.Arsenal.GetSkillContainer(MD.ContainerSlot.Left);
            rightContainer = localPlayer.Arsenal.GetSkillContainer(MD.ContainerSlot.Right);
        }

    }
}