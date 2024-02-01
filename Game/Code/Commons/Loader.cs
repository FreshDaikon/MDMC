using System.Runtime.Intrinsics.Arm;
using Godot;

public partial class Loader : Node
{
    [Export(PropertyHint.File)]
    private string ServerPath;
    [Export(PropertyHint.File)]
    private string ClientPath;

    public override void _Ready()
    {
        if(DisplayServer.GetName() == "headless")
        {            
            var res = (PackedScene)ResourceLoader.Load(ServerPath);
            var server = res.Instantiate();
            AddChild(server);
        }
        else
        {
            var arguments = new Godot.Collections.Dictionary();
            foreach (var argument in OS.GetCmdlineArgs())
            {
                if(argument.Contains("--resolution"))
                {
                    var value = argument.Replace("--resolution ", "");
                    var x = value.Split("x")[0];
                    var y = value.Split("x")[1];
                    DisplayServer.WindowSetSize(new Vector2I(int.Parse(x), int.Parse(y)));
                }
                if(argument.Contains("--position"))
                {
                    var value = argument.Replace("--position ", "");
                    var x = value.Split(",")[0];
                    var y = value.Split(",")[1];
                    DisplayServer.WindowSetPosition(new Vector2I(int.Parse(x), int.Parse(y)));
                }
            }
            var res = (PackedScene)ResourceLoader.Load(ClientPath);
            var client = res.Instantiate();
            AddChild(client);
        }
        base._Ready();
    }
}