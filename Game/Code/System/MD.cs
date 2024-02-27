using System;
using System.Collections.Generic;
using Godot;

namespace Daikon.Helpers;

public static class MD
{
    public enum ActionResult
    {
        CAST,
        IS_CASTING,
        CASTING_INTERUPTED,
        CASTING_FINISHED,
        CHANNEL,
        IS_CHANNELING,
        CHANNELING_INTERUPTED,
        CHANNELING_FINISHED,
        ON_COOLDOWN,
        CANT_STACK,
        MAX_STACKS,
        OUT_OF_RANGE,
        INVALID_TARGET,
        INSUFFICIENT_RESOURCE,
        NOT_SERVER,
        ERROR,
        INVALID_SETUP
    }
    public enum InputScheme
    {
        KEYBOARD,
        GAMEPAD
    }
    public enum CombatMessageType
    {
        DAMAGE,
        HEAL,
        ENMITY,
        EFFECT
    }
    public enum SkillType
    {
        TANK,
        DPS,
        HEAL
    }

    public enum SkillTimerType
    {
        GCD,
        OGCD
    }

    public enum SkillActionType
    {
        INSTANT,
        CAST,
        CHANNEL
    }

    public enum ContainerSlot
    {
        Main,
        Right,
        Left
    }

    public enum ModCategory
    {
        // Speed of the entity (movement speed)
        SPEED,
        // Max Health of the entity - flat values expected
        MAX_HEALTH,
        // Enmity multiplier of the entity multiplicatives expected ( 1.0 = 100% )
        ENMITY,
        // Base Mitts of the entity - flat multipliers expected (0.4 = 40% less damage taken, adds up so 0.4 + 0.4 = 80% less damage taken)
        BASE_MITS,
        // Active mitts of the entity - multiplicatives expected ( 0.8 = 20% less damage taken, multiplies so 0.8 * 0.8 = 36% less damage taken)
        ACTIVE_MITS,
        // Damage done by the entity - multiplicatives expected ( 1.2 = 20% more damage done, multiplies so 1.2 * 1.2 = 44% more damage done)
        DAMAGE_DONE,
        // Damage taken by the entity - flat multipliers expected ( 1.2 = 20% more damage taken, adds up so 1.2 + 1.2 = 40% more damage taken)
        DAMAGE_RECIEVED,
        // Healing done multipliers - flat multipliers expected ( 1.2 = 20% more healing done, adds up so 1.2 + 1.2 = 40% more healing done)
        HEAL_DONE,
        // Heal recieved multipliers - flat multipliers expected ( 1.2 = 20% more healing recieved, adds up so 1.2 + 1.2 = 40% more healing recieved).
        HEAL_RECIEVED,
        // GCD Mod - multiplies both ways ( 0.8 = 20% faster GCD, 1.2 = 20% slower GCD)
        GCD,
    }
    public enum ModType
    {
        ADDITIVE,
        MULTIPLICATIVE
    }

    public enum Rarity
    {
        COOL,
        AMAZING,
        AWESOME,
        EPIC
    }

    public enum RealizationMode
    {
        STATIC,
        DYNAMIC,
    }

    public enum RealizationSpawnType
    {
        POSITION,
        TRANSFORM,
        TARGET
    }

    public enum Runtime
    {
        Client,
        Server,
        SteamClient, 
        PlayfabServer,
        Offline,
        GAME
    }

    public enum WSConnectionState
    {
        Connecting,
        Open,
        Closing,
        Closed,
        RequestingNewGame,
        RequestingJoinGame,
    }
    public enum ClientGameState
    {
        Pregame,
        InArena,        
    }

    public static float Gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    public static Dictionary<string, string> GetArgs()
    {
        var args = new Dictionary<string, string>();        
        foreach (var argument in OS.GetCmdlineArgs())
        {
            string[] keyValue = argument.Split(" ");
            keyValue[0] = keyValue[0].Replace("--", "");
            args.Add(keyValue[0], keyValue.Length > 1 ? keyValue[1] : "");
        }
        return args;
    }

    public static string FormatDisplayNumber(float num)
	{
		 // Ensure number has max 3 significant digits (no rounding up can happen)
         long i = (long)Math.Pow(10, (int)Math.Max(0, Math.Log10(num) - 2));
         num = num / i * i;

         if (num >= 1000000000)
            return (num / 1000000000D).ToString("0.##") + "B";
         if (num >= 1000000)
            return (num / 1000000D).ToString("0.##") + "M";
         if (num >= 1000)
            return (num / 1000D).ToString("0.##") + "K";

         return num.ToString("#,0");
	}

    public static Color GetPlayerColor(float value)
    {
        var remap = Mathf.Remap(value, 3f, 48f, 0.0f, 1f);
        var newColor = Color.FromHsv(remap, 0.7f, 0.6f);
        return newColor; 
    }
}