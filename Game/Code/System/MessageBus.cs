using Godot;

public partial class MessageBus : Node
{

    public static MessageBus Instance;

    [Signal]
    public delegate void TestSignalEventHandler();
    [Signal]
    public delegate void ClientConnectedEventHandler();
    [Signal]
    public delegate void SkillUpdatedEventHandler();

    public override void _Ready()
    {
        if(Instance != null)
        {
            this.Free();
        }
        Instance = this;
    }
}