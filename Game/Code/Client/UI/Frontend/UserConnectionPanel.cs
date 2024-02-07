

using System.Data;
using Godot;
using Steamworks;

public partial class UserConnectionPanel : Control
{
    private string UserName = SteamClient.Name;
    private SteamId ID = SteamClient.SteamId;


    public override void _Ready()
    {
        
        base._Ready();
        
   
    }



}