using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace MalumMenu;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.LateUpdate))]
public static class PlayerPhysics_LateUpdate
{
    private static readonly Dictionary<byte, bool> wasInVent = new();
    public static readonly Dictionary<byte, Vector2> lastKnownPositions = new();

    public static void ClearAllStates()
    {
        wasInVent.Clear();
        lastKnownPositions.Clear();
    }

    public static void Postfix(PlayerPhysics __instance)
    {
        if (__instance.myPlayer != null && !__instance.myPlayer.Data.IsDead)
        {
            // Update the player's last known position every frame they are not in a vent.
            if (!__instance.myPlayer.inVent)
            {
                lastKnownPositions[__instance.myPlayer.PlayerId] = __instance.myPlayer.GetTruePosition();
            }

            // Vent usage detection
            if (CheatToggles.notifyOnVent && Utils.isInGame)
            {
                byte playerId = __instance.myPlayer.PlayerId;
                bool currentlyInVent = __instance.myPlayer.inVent;

                if (wasInVent.TryGetValue(playerId, out bool previouslyInVent) && currentlyInVent != previouslyInVent)
                {
                    Vector2 positionToCheck = currentlyInVent ? lastKnownPositions[playerId] : __instance.myPlayer.GetTruePosition();
                    PlainShipRoom room = Utils.getRoomFromPosition(positionToCheck);
                    string roomName = room != null ? room.RoomId.ToString() : "an unknown location";

                    NotificationHandler.HandleVent(__instance.myPlayer, currentlyInVent, roomName);
                }
                wasInVent[playerId] = currentlyInVent;
            }
        }

        MalumESP.playerNametags(__instance);
        MalumESP.seeGhostsCheat(__instance);

        MalumCheats.noClipCheat();
        MalumCheats.speedBoostCheat();
        MalumCheats.reviveCheat();
        MalumCheats.killAllCheat();
        MalumCheats.killAllCrewCheat();
        MalumCheats.killAllImpsCheat();
        MalumCheats.teleportCursorCheat();
        MalumCheats.completeMyTasksCheat();

        MalumPPMCheats.spectatePPM();
        MalumPPMCheats.killPlayerPPM();
        //MalumPPMCheats.telekillPlayerPPM();
        MalumPPMCheats.teleportPlayerPPM();
        MalumPPMCheats.changeRolePPM();

        //if (MalumPPMCheats.teleKillWaitFrames == 0){
        //    KillAnimation.SetMovement(PlayerControl.LocalPlayer, true);
        //    PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(MalumPPMCheats.teleKillPosition);
        //}

        //MalumPPMCheats.teleKillWaitFrames--;

        TracersHandler.drawPlayerTracer(__instance);

        GameObject[] bodyObjects = GameObject.FindGameObjectsWithTag("DeadBody");
        foreach(GameObject bodyObject in bodyObjects)
        {
            DeadBody deadBody = bodyObject.GetComponent<DeadBody>();
            if (deadBody && !deadBody.Reported)
            {
                TracersHandler.drawBodyTracer(deadBody);
            }
        }
    }
}