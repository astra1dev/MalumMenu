using UnityEngine;
using Il2CppSystem.Collections.Generic;

namespace MalumMenu;

public class DoorsUI : MonoBehaviour
{
    private Rect _windowRect = new(320, 10, 560, 320);
    private GUIStyle _windowStyle;
    private GUIStyle _separatorStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _toggleStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _headerStyle;
    private GUIStyle _statusStyle;
    private GUIStyle _footerStyle;
    private Texture2D _bgTex;
    private Texture2D _btnTex;
    private Texture2D _btnHoverTex;
    private Texture2D _accentTex;
    private List<SystemTypes> doorsToSpamOpen = new();
    private List<SystemTypes> doorsToSpamClose = new();
    private bool _stylesInit = false;
    private string[] _funnyTips = new string[]
    {
        "astra1dev probably forgot how doors work lmao",
        "doors go brrr - not sponsored by astra1dev",
        "astra1dev tried to close a door once... he's still pushing",
        "legend says astra1dev codes with his eyes closed",
        "astra1dev approved this UI (he didnt)",
        "fun fact: astra1dev thinks 'Open' means 'Close'",
        "astra1dev's favorite door? the logout button",
        "powered by astra1dev's tears of confusion",
        "modmenucrew devs crying looking at this masterpiece",
        "this UI is so good even modmenucrew wants to copy it",
        "modmenucrew could never achieve this level of door control",
        "rumor has it modmenucrew is taking notes rn",
        "modmenucrew wishes they had doors this beautiful",
        "astra1dev showed this to modmenucrew and they got jealous lol",
        "this door UI alone makes modmenucrew obsolete ngl"
    };
    private int _tipIndex = 0;
    private float _lastTipChange = 0f;

    private Texture2D MakeTex(int w, int h, Color col)
    {
        var pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        var tex = new Texture2D(w, h);
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }

    private void InitStyles()
    {
        if (_stylesInit) return;

        _bgTex = MakeTex(2, 2, new Color(0.08f, 0.08f, 0.12f, 0.97f));
        _btnTex = MakeTex(2, 2, new Color(0.18f, 0.22f, 0.35f, 1f));
        _btnHoverTex = MakeTex(2, 2, new Color(0.28f, 0.35f, 0.55f, 1f));
        _accentTex = MakeTex(2, 2, new Color(0.4f, 0.6f, 0.95f, 1f));

        _windowStyle = new GUIStyle(GUI.skin.window)
        {
            normal = { background = _bgTex, textColor = new Color(0.9f, 0.92f, 1f) },
            onNormal = { background = _bgTex, textColor = new Color(0.9f, 0.92f, 1f) },
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            padding = { left = 12, right = 12, top = 25, bottom = 12 },
            border = { left = 8, right = 8, top = 8, bottom = 8 }
        };

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { background = _btnTex, textColor = Color.white },
            hover = { background = _btnHoverTex, textColor = new Color(0.85f, 0.9f, 1f) },
            active = { background = _accentTex, textColor = Color.white },
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            padding = { left = 8, right = 8, top = 5, bottom = 5 },
            margin = { left = 3, right = 3, top = 2, bottom = 2 }
        };

        _toggleStyle = new GUIStyle(GUI.skin.toggle)
        {
            normal = { textColor = new Color(0.75f, 0.8f, 0.95f) },
            onNormal = { textColor = new Color(0.5f, 0.85f, 0.5f) },
            hover = { textColor = new Color(0.9f, 0.95f, 1f) },
            onHover = { textColor = new Color(0.6f, 1f, 0.6f) },
            fontSize = 11,
            fontStyle = FontStyle.Normal,
            padding = { left = 18, right = 4, top = 2, bottom = 2 }
        };

        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = new Color(0.85f, 0.88f, 0.98f) },
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };

        _statusStyle = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = new Color(0.6f, 0.65f, 0.8f) },
            fontSize = 11,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleLeft
        };

        _headerStyle = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = new Color(0.5f, 0.7f, 1f) },
            fontSize = 10,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleCenter
        };

        _footerStyle = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = new Color(0.45f, 0.5f, 0.65f) },
            fontSize = 9,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleCenter
        };

        _separatorStyle = new GUIStyle()
        {
            normal = { background = _accentTex },
            margin = { left = 0, right = 0, top = 6, bottom = 6 }
        };

        _stylesInit = true;
    }

    private void OnGUI()
    {
        if (!CheatToggles.showDoorsMenu) return;

        InitStyles();

        _windowRect = GUI.Window(2, _windowRect, (GUI.WindowFunction)DoorsWindow, "Door Control Panel", _windowStyle);
    }

    private void DoorsWindow(int windowID)
    {
        if (!Utils.isShip)
        {
            GUILayout.Label("yo get in a game first lol - astra1dev probably", _headerStyle);
            GUI.DragWindow();
            return;
        }

        var map = (MapNames)Utils.getCurrentMapID();

        if (map is MapNames.MiraHQ)
        {
            GUILayout.Label("MiraHQ has no doors... just like astra1dev has no brain cells", _headerStyle);
            GUI.DragWindow();
            return;
        }

        if (Time.time - _lastTipChange > 4f)
        {
            _tipIndex = (_tipIndex + 1) % _funnyTips.Length;
            _lastTipChange = Time.time;
        }

        GUILayout.BeginVertical();

        GUILayout.Label(_funnyTips[_tipIndex], _headerStyle);
        GUILayout.Space(4);
        GUILayout.Box("", _separatorStyle, GUILayout.Height(2f), GUILayout.ExpandWidth(true));
        GUILayout.Space(4);

        foreach (var doorRoom in DoorsHandler.GetDoorRooms())
        {
            GUILayout.BeginHorizontal();
            
            GUILayout.Label($"» {doorRoom}", _labelStyle, GUILayout.Width(110f));
            GUILayout.Label($"{DoorsHandler.GetStatusOfDoorsInRoom(doorRoom, true)}", _statusStyle, GUILayout.Width(70f));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Lock", _buttonStyle, GUILayout.Width(50f)))
            {
                DoorsHandler.CloseDoorsOfRoom(doorRoom);
            }

            if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
            {
                if (GUILayout.Button("Open", _buttonStyle, GUILayout.Width(50f)))
                {
                    DoorsHandler.OpenDoorsOfRoom(doorRoom);
                }
            }

            if (Utils.isHost)
            {
                var spamClose = doorsToSpamClose.Contains(doorRoom);
                spamClose = GUILayout.Toggle(spamClose, "Spam Lock", _toggleStyle);
                if (spamClose && !doorsToSpamClose.Contains(doorRoom))
                    doorsToSpamClose.Add(doorRoom);
                else if (!spamClose && doorsToSpamClose.Contains(doorRoom))
                    doorsToSpamClose.Remove(doorRoom);

                if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
                {
                    var spamOpen = doorsToSpamOpen.Contains(doorRoom);
                    spamOpen = GUILayout.Toggle(spamOpen, "Spam Open", _toggleStyle);
                    if (spamOpen && !doorsToSpamOpen.Contains(doorRoom))
                        doorsToSpamOpen.Add(doorRoom);
                    else if (!spamOpen && doorsToSpamOpen.Contains(doorRoom))
                        doorsToSpamOpen.Remove(doorRoom);
                }
            }
            else
            {
                if (doorsToSpamClose.Count != 0 || doorsToSpamOpen.Count != 0)
                {
                    doorsToSpamClose.Clear();
                    doorsToSpamOpen.Clear();
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(2);
        }

        GUILayout.FlexibleSpace();
        GUILayout.Box("", _separatorStyle, GUILayout.Height(2f), GUILayout.ExpandWidth(true));
        GUILayout.Space(4);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Lock All Doors", _buttonStyle))
        {
            CheatToggles.closeAllDoors = true;
        }

        if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
        {
            if (GUILayout.Button("Open All Doors", _buttonStyle))
            {
                CheatToggles.openAllDoors = true;
            }
        }

        GUILayout.FlexibleSpace();

        if (Utils.isHost)
        {
            CheatToggles.spamCloseAllDoors = GUILayout.Toggle(CheatToggles.spamCloseAllDoors, "Spam Lock All", _toggleStyle);

            if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
            {
                CheatToggles.spamOpenAllDoors = GUILayout.Toggle(CheatToggles.spamOpenAllDoors, "Spam Open All", _toggleStyle);
            }
        }
        else
        {
            CheatToggles.spamCloseAllDoors = CheatToggles.spamOpenAllDoors = false;
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        GUILayout.Label("astra1dev seal of disapproval™ - he tried his best (he didnt)", _footerStyle);

        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    public void Update()
    {
        if (!Utils.isShip) return;

        foreach (var doorRoom in doorsToSpamClose)
        {
            DoorsHandler.CloseDoorsOfRoom(doorRoom);
        }

        var map = (MapNames)Utils.getCurrentMapID();

        if (map is MapNames.Polus or MapNames.Airship or MapNames.Fungle)
        {
            foreach (var doorRoom in doorsToSpamOpen)
            {
                DoorsHandler.OpenDoorsOfRoom(doorRoom);
            }
        }
    }
}
