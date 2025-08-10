using UnityEngine;

namespace MalumMenu;

/// <summary>
/// Renders the Notification Log window using OnGUI.
/// </summary>
public class NotificationUI : MonoBehaviour
{
    private Vector2 scrollPosition = Vector2.zero;
    private Rect windowRect = new Rect(320, 10, 500, 300);
    private GUIStyle logStyle;

    private void OnGUI()
    {
        if (!CheatToggles.showNotificationLog) return;

        if (logStyle == null){
            logStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                richText = true // Essential for colored names
            };
        }

        Color configUIColor;
        if (ColorUtility.TryParseHtmlString(MalumMenu.menuHtmlColor.Value, out configUIColor)){
            GUI.backgroundColor = configUIColor;
        }

        windowRect = GUI.Window(2, windowRect, (GUI.WindowFunction)NotificationWindow, "Notification Log");
    }

    private void NotificationWindow(int windowID)
    {
        GUILayout.BeginVertical();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

        foreach (var log in NotificationHandler.notificationLog)
        {
            GUILayout.Label(log, logStyle);
        }

        GUILayout.EndScrollView();

        if (GUILayout.Button("Clear Log")){
            NotificationHandler.notificationLog.Clear();
        }

        GUILayout.EndVertical();

        GUI.DragWindow();
    }
}