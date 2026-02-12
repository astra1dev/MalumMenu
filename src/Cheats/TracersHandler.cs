using UnityEngine;

namespace MalumMenu;
public static class TracersHandler
{
    public static void drawPlayerTracer(PlayerPhysics playerPhysics){
        try{

            Color color = Color.clear; // All tracers are invisible by default

            if (!playerPhysics.myPlayer.Data.IsDead){
                if (CheatToggles.tracersCrew && !playerPhysics.myPlayer.Data.Role.IsImpostor){
                    if (CheatToggles.distanceBasedTracers){
                        color = GetDistanceBasedColor(playerPhysics.myPlayer.transform.position);
                    }else if (CheatToggles.colorBasedTracers){
                        color = playerPhysics.myPlayer.Data.Color; // Color-Based Tracer
                    }else{
                        color = playerPhysics.myPlayer.Data.Role.TeamColor; // Team-Based Tracer
                    }
                }else if (CheatToggles.tracersImps && playerPhysics.myPlayer.Data.Role.IsImpostor){
                    if (CheatToggles.distanceBasedTracers){
                        color = GetDistanceBasedColor(playerPhysics.myPlayer.transform.position);
                    }else if (CheatToggles.colorBasedTracers){
                        color = playerPhysics.myPlayer.Data.Color; // Color-Based Tracer
                    }else{
                        color = playerPhysics.myPlayer.Data.Role.TeamColor; // Team-Based Tracer
                    }
                }
            }else{
                if (CheatToggles.tracersGhosts){
                    if (CheatToggles.distanceBasedTracers){
                        color = GetDistanceBasedColor(playerPhysics.myPlayer.transform.position);
                    }else if (CheatToggles.colorBasedTracers){
                        color = playerPhysics.myPlayer.Data.Color; // Color-Based Tracer
                    }else{
                        color = Palette.White; // Ghost Tracer (White)
                    }
                }
            }

            // Draw tracer between the player and LocalPlayer using the right color
            Utils.drawTracer(playerPhysics.myPlayer.gameObject, PlayerControl.LocalPlayer.gameObject, color);

        }catch{}
    }

    public static void drawBodyTracer(DeadBody deadBody){
        Color color = Color.clear; // All tracers are invisible by default

        if (CheatToggles.tracersBodies){
            if (CheatToggles.distanceBasedTracers){
                color = GetDistanceBasedColor(deadBody.transform.position);
            }else if (CheatToggles.colorBasedTracers){

                // Fetch the dead body's PlayerInfo
                NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(deadBody.ParentId);

                color = playerById.Color; // Color-Based Tracer

            }else{

                color = Color.yellow; // Dead Body Tracer (Yellow)

            }
        }

        // Draw tracer between the dead body and LocalPlayer using the right color
        Utils.drawTracer(deadBody.gameObject, PlayerControl.LocalPlayer.gameObject, color);
    }

    // Red-Yellow-Green cycle: Red = close, Yellow = medium, Green = far
    private static Color GetDistanceBasedColor(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(targetPosition, PlayerControl.LocalPlayer.transform.position);

        float maxDistance = 20f; // Green at 20+ units
        float minDistance = 2f;  // Red at 2 units or less

        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        float normalized = (distance - minDistance) / (maxDistance - minDistance);

        float r;
        float g;
        float b = 0f;

        if (normalized < 0.5f)
        {
            float t = normalized * 2f;
            r = 1f;
            g = t;
        }
        else
        {
            float t = (normalized - 0.5f) * 2f;
            r = 1f - t;
            g = 1f;
        }

        return new Color(r, g, b, 1f);
    }
}
