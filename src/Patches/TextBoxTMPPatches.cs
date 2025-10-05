using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.Update))]
public static class TextBoxTMP_Update
{
    // Postfix patch of TextBoxTMP.Update to allow copying from the chatbox
    public static void Postfix(TextBoxTMP __instance)
    {
        if (CheatToggles.chatJailbreak)
        { 
            if (!__instance.hasFocus){return;}

            // If the user is pressing Ctrl + C, copy the text from the chatbox to the device's clipboard
            if((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.C))
            {
                ClipboardHelper.PutClipboardString(__instance.text);
            }
        }
    }
}

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.IsCharAllowed))]
public static class TextBoxTMP_IsCharAllowed
{
    public static bool Prefix(TextBoxTMP __instance, char i, ref bool __result)
    {
        if (CheatToggles.chatJailbreak)
        {
            // Block only *actual* control characters and obvious UI-breakers
            HashSet<char> blockedSymbols = new() { '\b', '\r' /* no < or > */ };

            if (blockedSymbols.Contains(i))
            {
                __result = false;
                return false;
            }

            __result = true;
            return false; // Allow all others, including layout-wonky characters
        }

        return true;
    }
}