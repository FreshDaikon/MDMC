using System;
using System.Linq;
using Godot;

/// <summary>
/// Reads input of the player and sync it over the network.<br/>
/// Should be given MultiplayerAuth(LocalPlayer) on creation.<br/>
/// </summary>
public partial class SyncTest : MultiplayerSynchronizer
{
    [Export]
    public double value = 0f;

    public override void _Process(double delta)
    {
        value += delta;
    }
}