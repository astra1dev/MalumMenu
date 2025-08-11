using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;

namespace MalumMenu;

/// <summary>
/// Handles the creation and storage of event-based notification logs.
/// </summary>
public static class NotificationHandler
{
    public static readonly List<string> notificationLog = new();
    private const int MaxLogEntries = 100;

    private static void AddLog(string message)
    {
        if (notificationLog.Count >= MaxLogEntries)
        {
            notificationLog.RemoveAt(0);
        }
        notificationLog.Add(message);

        // Also show the notification on the bottom-left of the screen.
        if (DestroyableSingleton<HudManager>.InstanceExists && HudManager.Instance.Notifier != null)
        {
            // We use AddDisconnectMessage as it's a simple way to show custom text.
            HudManager.Instance.Notifier.AddDisconnectMessage(message);
        }
    }

    /// <summary>
    /// Checks if a player is currently disguised and returns their real and displayed names.
    /// </summary>
    private static (string realName, string displayName, bool isDisguised) GetPlayerIdentity(PlayerControl player)
    {
        if (player == null || player.Data == null) return ("", "", false);

        // Get the real player's name and color from their permanent data
        string realName = $"<color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{player.Data.PlayerName}</color>";

        // Get the displayed (current outfit) name and color
        string displayName = $"<color=#{ColorUtility.ToHtmlStringRGB(Palette.PlayerColors[player.CurrentOutfit.ColorId])}>{player.CurrentOutfit.PlayerName}</color>";

        // A player is disguised if their currently displayed name is different from their real name.
        // This is the most reliable way to check for shapeshifting on ALL clients,
        // as the 'CurrentOutfit' is synchronized for rendering purposes, whereas specific
        // role timers (like durationSecondsRemaining) might not be.
        bool isDisguised = player.CurrentOutfit.PlayerName != player.Data.PlayerName;

        return (realName, displayName, isDisguised);
    }

    public static void HandlePlayerKill(PlayerControl killer, PlayerControl victim)
    {
        if (!CheatToggles.notifyOnDeath || killer == null || victim == null) return;

        var (realKillerName, displayKillerName, isDisguised) = GetPlayerIdentity(killer);
        string victimName = $"<color=#{ColorUtility.ToHtmlStringRGB(victim.Data.Color)}>{victim.CurrentOutfit.PlayerName}</color>";

        PlainShipRoom room = Utils.getRoomFromPosition(victim.GetTruePosition());
        string roomName = room != null ? room.RoomId.ToString() : "an unknown location";

        string message;
        if (isDisguised)
        {
            message = $"{realKillerName} (as {displayKillerName}) killed {victimName} in {roomName}.";
        }
        else
        {
            message = $"{realKillerName} killed {victimName} in {roomName}.";
        }
        AddLog(message);
    }

    public static void HandleGuardianAngelSave(PlayerControl killer, PlayerControl target)
    {
        if (!CheatToggles.notifyOnDeath || killer == null || target == null) return;

        var (realKillerName, displayKillerName, isDisguised) = GetPlayerIdentity(killer);
        string targetName = $"<color=#{ColorUtility.ToHtmlStringRGB(target.Data.Color)}>{target.CurrentOutfit.PlayerName}</color>";

        PlainShipRoom room = Utils.getRoomFromPosition(target.GetTruePosition());
        string roomName = room != null ? room.RoomId.ToString() : "an unknown location";

        string message;
        if (isDisguised)
        {
            message = $"{realKillerName} (as {displayKillerName}) tried to kill {targetName} in {roomName}. (Saved)";
        }
        else
        {
            message = $"{realKillerName} tried to kill {targetName} in {roomName}. (Saved)";
        }
        AddLog(message);
    }

    public static void HandleVent(PlayerControl player, bool entered, string roomName)
    {
        if (!CheatToggles.notifyOnVent || player == null) return;

        var (realPlayerName, displayPlayerName, isDisguised) = GetPlayerIdentity(player);
        string action = entered ? "entered" : "exited";

        string message;
        if (isDisguised)
        {
            message = $"{realPlayerName} (as {displayPlayerName}) has {action} a vent in {roomName}.";
        }
        else
        {
            message = $"{realPlayerName} has {action} a vent in {roomName}.";
        }
        AddLog(message);
    }

    public static void HandlePlayerDisconnect(NetworkedPlayerInfo player)
    {
        if (!CheatToggles.notifyOnDisconnect || player == null) return;

        string playerName = $"<color=#{ColorUtility.ToHtmlStringRGB(player.Color)}>{player.PlayerName}</color>";

        string message = $"{playerName} has disconnected.";
        AddLog(message);
    }
}