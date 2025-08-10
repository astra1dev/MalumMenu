using System.Collections.Generic;
using UnityEngine;

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
    }

    public static void HandlePlayerKill(PlayerControl killer, PlayerControl victim)
    {
        if (!CheatToggles.notifyOnDeath || killer == null || victim == null) return;

        string killerName = $"<color=#{ColorUtility.ToHtmlStringRGB(killer.Data.Color)}>{killer.CurrentOutfit.PlayerName}</color>";
        string victimName = $"<color=#{ColorUtility.ToHtmlStringRGB(victim.Data.Color)}>{victim.CurrentOutfit.PlayerName}</color>";

        PlainShipRoom room = Utils.getRoomFromPosition(victim.GetTruePosition());
        string roomName = room != null ? room.RoomId.ToString() : "an unknown location";

        string message = $"{killerName} killed {victimName} in {roomName}.";
        AddLog(message);
    }

    public static void HandleVent(PlayerControl player, bool entered, string roomName)
    {
        if (!CheatToggles.notifyOnVent || player == null) return;

        string playerName = $"<color=#{ColorUtility.ToHtmlStringRGB(player.Data.Color)}>{player.CurrentOutfit.PlayerName}</color>";
        string action = entered ? "entered" : "exited";

        string message = $"{playerName} has {action} a vent in {roomName}.";
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