using HarmonyLib;
using System.Collections.Generic;

namespace MalumMenu;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class PlayerControl_FixedUpdate
{
    public static void Postfix(PlayerControl __instance){

        if (__instance.AmOwner){
            MalumCheats.noKillCdCheat(__instance);
        }

    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class PlayerControl_MurderPlayer_Patch
{
    // A HashSet to track victims for whom a notification has already been sent on this client.
    // This prevents duplicate notifications if the event is somehow triggered more than once.
    private static readonly HashSet<byte> notifiedKilledVictims = new();

    /// <summary>
    /// Clears the set of notified victims. This must be called at the end of each game.
    /// </summary>
    public static void ClearNotifiedKilledVictims() => notifiedKilledVictims.Clear();

    // A Prefix runs *before* the original method. This lets us check conditions before the kill happens.
    public static void Prefix(PlayerControl __instance, PlayerControl target)
    {
        if (target == null)
        {
            return;
        }

        // Check if the target is protected by a Guardian Angel.
        if (target.protectedByGuardianId != -1)
        {
            // This is a "save" event. Show the notification but do not add the player to the
            // notifiedKilledVictims set, allowing a future kill notification to appear.
            NotificationHandler.HandleGuardianAngelSave(__instance, target);
        }
        else
        {
            // This is a potential kill event. Check if we've already notified for this victim's death.
            if (notifiedKilledVictims.Contains(target.PlayerId))
            {
                return;
            }

            // If not protected and not already notified, the kill is successful.
            NotificationHandler.HandlePlayerKill(__instance, target);

            // Add the victim's ID to the set ONLY on a successful kill to prevent duplicate kill notifications.
            notifiedKilledVictims.Add(target.PlayerId);
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckMurder))]
public static class PlayerControl_CmdCheckMurder
{
    // Prefix patch of PlayerControl.CmdCheckMurder to always bypass checks when killing players
    public static bool Prefix(PlayerControl __instance, PlayerControl target)
    {
        /*if (Utils.isLobby){
            HudManager.Instance.Notifier.AddDisconnectMessage("Killing in lobby disabled for being too buggy");
            return false;
        }

        // Direct kill RPC should only be used when absolutely necessary as to avoid detection from anticheat mods
        if (!CheatToggles.killAnyone && !CheatToggles.zeroKillCd && !Utils.isVanished(__instance.Data) &&
            !Utils.isMeeting &&
            (MalumPPMCheats.oldRole == null ||
             Utils.getBehaviourByRoleType((AmongUs.GameOptions.RoleTypes)MalumPPMCheats.oldRole).IsImpostor))
            return true;
        if (!__instance.Data.Role.IsValidTarget(target.Data))
        {
            return true;
        }

        if (target.protectedByGuardianId > -1 && !CheatToggles.killAnyone){
            return true;
        }

        Utils.murderPlayer(target, MurderResultFlags.Succeeded);

        return false;*/

        if (!Utils.isHost) return true;
        // __instance.isKilling = true;
        PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
        return false;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TurnOnProtection))]
public static class PlayerControl_TurnOnProtection
{
    // Prefix patch of PlayerControl.ProtectPlayer to render all protections visible if CheatToggles.seeGhosts is enabled
    public static void Prefix(ref bool visible){
        if (CheatToggles.seeGhosts){
            visible = true;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckShapeshift))]
public static class ShapeshifterCheats_PlayerControl_CmdCheckShapeshift_Postfix
{
    // Prefix patch of PlayerControl.CmdCheckShapeshift to prevent SS animation
    public static void Prefix(ref bool shouldAnimate){

        if (shouldAnimate && CheatToggles.noShapeshiftAnim){
            shouldAnimate = false;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckRevertShapeshift))]
public static class ShapeshifterCheats_PlayerControl_CmdCheckRevertShapeshift_Postfix
{
    // Prefix patch of PlayerControl.CmdCheckRevertShapeshift to prevent SS animation
    public static void Prefix(ref bool shouldAnimate){

        if (shouldAnimate && CheatToggles.noShapeshiftAnim){
            shouldAnimate = false;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public static class NoAntiCheat_PlayerControl_RpcSyncSettings_Prefix
{
    /// <summary>
    /// Prevent the anti-cheat from kicking you for some settings that are out of the "original" valid range.
    /// </summary>
    /// <param name="__instance">The <c>PlayerControl</c> instance.</param>
    /// <param name="optionsByteArray">The byte array containing the options to sync.</param>
    /// <returns><c>false</c> to skip the original method, <c>true</c> to allow the original method to run.</returns>
    public static bool Prefix(PlayerControl __instance, byte[] optionsByteArray)
    {
        return !CheatToggles.noOptionsLimits;
    }
}
