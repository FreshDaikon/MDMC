using Godot;

public partial class Loader : Node
{
    //Dependency Scenes:
    [Export(PropertyHint.File)]
    private string ServerPath;
    [Export(PropertyHint.File)]
    private string ClientPath;
   
    public override void _Ready()
    {
        GD.Print("Starting MDMC");
        var args = MD.GetArgs();
        GD.Print(args);
        //First check for Server:
        if(args.ContainsKey("gameserver"))
        {
            var ServerSceneRes = (PackedScene)ResourceLoader.Load(ServerPath);
            var server = ServerSceneRes.Instantiate();
            AddChild(server);
        }
        else if(args.ContainsKey("playfab"))
        {
            var ServerSceneRes = (PackedScene)ResourceLoader.Load(ServerPath);
            var server = ServerSceneRes.Instantiate();
            AddChild(server);
        }
        // We are a Client:
        else
        {
            GD.Print("Starting as client..");
            var ClientSceneRes = (PackedScene)ResourceLoader.Load(ClientPath);
            var client = ClientSceneRes.Instantiate();
            AddChild(client);
        }
    }
}