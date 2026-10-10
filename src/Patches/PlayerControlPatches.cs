using HarmonyLib;
using Il2CppSystem;
using UnityEngine;
using Object = UnityEngine.Object;
using TMPro;

namespace MalumMenu;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static class PlayerControl_SetKillTimer
{
    // Prefix patch of PlayerControl.SetKillTimer to remove kill cooldown
    public static void Prefix(PlayerControl __instance, ref float time)
    {
        if (!__instance.AmOwner || !Utils.isHost || !CheatToggles.noKillCd) return;

        time = 0f;
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

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class PlayerControl_MurderPlayer
{
    // Prefix patch of PlayerControl.MurderPlayer to log on ConsoleUI when a player tries to kill another player,
    // along with who the killer and target are, and where the kill happened.
    // Also logs when a kill gets saved by a guardian angel.
    public static void Prefix(PlayerControl __instance, PlayerControl target)
    {
        if (!CheatToggles.logDeaths || target == null) return;

        var (realKillerName, displayKillerName, isDisguised) = Utils.GetPlayerIdentity(__instance);
        var targetName = $"<color=#{ColorUtility.ToHtmlStringRGB(target.Data.Color)}>{target.CurrentOutfit.PlayerName}</color>";

        var room = Utils.GetRoomFromPosition(target.GetTruePosition());
        var roomName = room != null ? room.RoomId.ToString() : "an unknown location";

        if (target.protectedByGuardianId != -1)
        {
            ConsoleUI.Log(isDisguised ? $"{realKillerName} (as {displayKillerName}) tried to kill {targetName} in {roomName} (Protected)"
                : $"{realKillerName} tried to kill {targetName} in {roomName} (Protected)");
        }
        else
        {
            ConsoleUI.Log(isDisguised ? $"{realKillerName} (as {displayKillerName}) killed {targetName} in {roomName}"
                : $"{realKillerName} killed {targetName} in {roomName}");
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TurnOnProtection))]
public static class PlayerControl_TurnOnProtection
{
    // Prefix patch of PlayerControl.TurnOnProtection to make all protections visible
    public static void Prefix(ref bool visible)
    {
		if (CheatToggles.seeGhosts)
        {
            visible = true;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckShapeshift))]
public static class PlayerControl_CmdCheckShapeshift
{
    // Prefix patch of PlayerControl.CmdCheckShapeshift to prevent SS animation
    public static void Prefix(ref bool shouldAnimate)
    {
        if (shouldAnimate && CheatToggles.noShapeshiftAnim)
        {
            shouldAnimate = false;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckRevertShapeshift))]
public static class PlayerControl_CmdCheckRevertShapeshift
{
    // Prefix patch of PlayerControl.CmdCheckRevertShapeshift to prevent SS animation
    public static void Prefix(ref bool shouldAnimate){

        if (shouldAnimate && CheatToggles.noShapeshiftAnim)
        {
            shouldAnimate = false;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class PlayerControl_Shapeshift
{
    // Postfix patch of PlayerControl.Shapeshift to log on ConsoleUI when a player shapeshifts into another player,
    // and who they shapeshifted into. Also logs when a shapeshift gets reverted.
    public static void Postfix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
    {
        if (!CheatToggles.logShapeshifts) return;

        if (__instance.CurrentOutfitType == PlayerOutfitType.MushroomMixup) return;

        var targetPlayerInfo = targetPlayer.Data;

        var room = Utils.GetRoomFromPosition(__instance.GetTruePosition());
        var roomName = room != null ? room.RoomId.ToString() : "an unknown location";

        if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)
        {
            ConsoleUI.Log($"<color=#{ColorUtility.ToHtmlStringRGB(GameData.Instance.GetPlayerById(__instance.PlayerId).Color)}>" +
                          $"{GameData.Instance.GetPlayerById(__instance.PlayerId)._object.Data.PlayerName}</color> unshapeshifted in {roomName}");
        }
        else
        {
            ConsoleUI.Log($"<color=#{ColorUtility.ToHtmlStringRGB(GameData.Instance.GetPlayerById(__instance.PlayerId).Color)}>" +
                          $"{GameData.Instance.GetPlayerById(__instance.PlayerId)._object.Data.PlayerName}</color> shapeshifted into " +
                          $"<color=#{ColorUtility.ToHtmlStringRGB(GameData.Instance.GetPlayerById(targetPlayerInfo.PlayerId).Color)}>" +
                          $"{GameData.Instance.GetPlayerById(targetPlayerInfo.PlayerId)._object.Data.PlayerName}</color> in {roomName}");
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
public static class PlayerControl_CompleteTask
{
    public static void Postfix(PlayerControl __instance, uint idx)
    {
        if (!CheatToggles.logTasks) return;

        var task = __instance.myTasks.Find((Predicate<PlayerTask>)(p => (int)p.Id == (int)idx));
        var room = Utils.GetRoomFromPosition(__instance.GetTruePosition());
        var roomName = room != null ? room.RoomId.ToString() : "an unknown location";

        if (task)
        {
            ConsoleUI.Log(
                $"<color=#{ColorUtility.ToHtmlStringRGB(__instance.Data.Color)}>{__instance.Data.PlayerName}</color> completed task {task.TaskType} in {roomName}");
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public static class PlayerControl_RpcSyncSettings
{
    // Prefix patch of PlayerControl.RpcSyncSettings to prevent the anti-cheat from kicking you
    // for some settings that are out of the "original" valid range
    public static bool Prefix(PlayerControl __instance, byte[] optionsByteArray)
    {
        return !CheatToggles.noOptionsLimits;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
public static class PlayerControl_Start
{
    public static GameObject LevelIcon;
    public static GameObject IdIcon;
    public static GameObject HostIcon;

    public static void Postfix(PlayerControl __instance)
    {
        // from https://github.com/WanderingPix/Host-Utility/blob/auos-api/HostUtility/Components/PlayerIconsBehaviour.cs

        var iconContainer = new GameObject("IconContainer");
        iconContainer.transform.SetParent(__instance.cosmetics.nameTextContainer.transform);
        iconContainer.transform.localPosition = __instance.cosmetics.nameText.transform.localPosition; // + new Vector3(0, 0.35f, 0);
        //__instance.StartCoroutine(Effects.ActionAfterDelay(0.25f,
        //    new System.Action(() => iconContainer.AddComponent<PlayerIconsBehaviour>().Initialize(__instance))));

        __instance.StartCoroutine(Effects.ActionAfterDelay(0.75f, new System.Action(() => {

            // Level Icon
            LevelIcon = Object.Instantiate(HudManager.Instance.MeetingPrefab.PlayerButtonPrefab.LevelNumberText.transform.parent, iconContainer.transform).gameObject;
            //LevelIcon.transform.GetChild(1).GetComponent<TextMeshPro>().text = "Guest";
            //LevelIcon.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.8f, 0.2f, 1f);
            LevelIcon.transform.GetChild(1).GetComponent<TextMeshPro>().text = __instance.Data.PlayerLevel.ToString();

            // ID Icon
            IdIcon = Object.Instantiate(HudManager.Instance.MeetingPrefab.PlayerButtonPrefab.LevelNumberText.transform.parent, iconContainer.transform).gameObject;
            IdIcon.transform.GetChild(1).GetComponent<TextMeshPro>().text = __instance.Data.PlayerId.ToString();

            var headerText = IdIcon.transform.GetChild(0);
            //headerText.GetComponent<TextTranslatorTMP>().Destroy();
            Object.Destroy(headerText.GetComponent<TextTranslatorTMP>());
            headerText.GetComponent<TextMeshPro>().text = "ID";
            IdIcon.GetComponent<SpriteRenderer>().color = Color.magenta;

            HostIcon = Object.Instantiate(HudManager.Instance.MeetingPrefab.PlayerButtonPrefab.LevelNumberText.transform.parent, iconContainer.transform).gameObject;
            var text = HostIcon.transform.GetChild(1);
            text.GetComponent<TextMeshPro>().text = "★";
            text.transform.localPosition = Vector3.zero;
            var headerText2 = HostIcon.transform.GetChild(0);
            Object.Destroy(headerText2.GetComponent<TextTranslatorTMP>());
            //headerText2.GetComponent<TextTranslatorTMP>().Destroy();
            headerText2.GetComponent<TextMeshPro>().text = "";
            HostIcon.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0, 1, 1);

            /*foreach (var go in GetComponentsInChildren<Transform>())
            {
                go.gameObject.layer = 0;
            }*/

        })));
    }
}
