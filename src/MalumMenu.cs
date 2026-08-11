using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine.SceneManagement;
using System;
using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace MalumMenu;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class MalumMenu : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static MalumMenu Plugin;
    public new static ManualLogSource Log;
    public static readonly string ProfilePath = Path.Combine(Paths.ConfigPath, "MalumProfile.txt");

    public static MenuUI menuUI;
    public static ConsoleUI consoleUI;
    public static RolesUI rolesUI;
    public static OverloadUI overloadUI;
    public static DoorsUI doorsUI;
    public static TasksUI tasksUI;
    public static ProtectUI protectUI;
    public static KeybindListener keybindListener;

    public static string malumVersion = "3.2.0";
    public static List<string> supportedAU = new List<string> { "2026.6.5", "2026.3.31" };
    public static bool isPanicked = false;
    public static bool inStealthMode = false;

    public static ConfigEntry<string> menuKeybind;
    public static ConfigEntry<string> menuHtmlColor;
    public static ConfigEntry<bool> menuOpenOnMouse;
    public static ConfigEntry<bool> menuKeepSubwindowsOpen;
    public static ConfigEntry<string> spoofLevel;
    public static ConfigEntry<string> spoofPlatform;
    public static ConfigEntry<bool> spoofDeviceId;
    public static ConfigEntry<bool> noTelemetry;
    public static ConfigEntry<string> guestFriendCode;
    public static ConfigEntry<bool> guestMode;
    public static ConfigEntry<bool> autoLoadProfile;
    public static ConfigEntry<string> configEditor;
    public static ConfigEntry<int> adaptMaxStrength;
    public static ConfigEntry<float> adaptMaxCooldown;
    public static ConfigEntry<float> attackLogDelay;
    public static ConfigEntry<int> defaultStrength;
    public static ConfigEntry<float> defaultCooldown;
    public static ConfigEntry<int> killSwitchLvl;

    public static UIBase UIBase { get; private set; }

    public override void Load()
    {
        Log = base.Log;
        Plugin = this;

        // Loads config settings
        menuKeybind = Config.Bind("MalumMenu.GUI",
                                "Keybind",
                                "Delete",
                                "The keyboard key used to toggle the GUI on and off. List of supported keycodes: https://docs.unity3d.com/Packages/com.unity.tiny@0.16/api/Unity.Tiny.Input.KeyCode.html");

        menuHtmlColor = Config.Bind("MalumMenu.GUI",
                                "Color",
                                "",
                                "A custom color for your MalumMenu GUI. Supports html color codes");

        menuOpenOnMouse = Config.Bind("MalumMenu.GUI",
                                "OpenOnMouse",
                                false,
                                "When enabled, the MalumMenu GUI will always be opened at the current mouse position");

        menuKeepSubwindowsOpen = Config.Bind("MalumMenu.GUI",
                                "KeepSubwindowsOpen",
                                false,
                                "When enabled, closing the MalumMenu GUI will not automatically close its subwindows");

        autoLoadProfile = Config.Bind("MalumMenu.Profile",
                                "AutoLoadProfile",
                                false,
                                "When enabled, your saved keybind and toggle profile will be automatically loaded at game startup");

        configEditor = Config.Bind("MalumMenu.Config",
                                "ConfigEditor",
                                "notepad.exe",
                                "The program used to open the config file when using the Open Config toggle. Can be any executable, but using a text editor is recommended");

        // GuestMode config settings are commented out as the cheats are broken in latest updates

        // guestMode = Config.Bind("MalumMenu.GuestMode",
        //                         "GuestMode",
        //                         false,
        //                         "When enabled, a new guest account will generate every time you start the game, allowing you to bypass account bans and PUID detection");

        // guestFriendCode = Config.Bind("MalumMenu.GuestMode",
        //                         "FriendName",
        //                         "",
        //                         "The username that will be used when setting a friend code for your guest account. IMPORTANT: Can only be used with GuestMode, needs to be ≤ 10 characters, and cannot include special characters/discriminator (#1234)");

        spoofLevel = Config.Bind("MalumMenu.Spoofing",
                                "Level",
                                "",
                                "A custom player level to display to others in online games to hide your actual platform. IMPORTANT: Custom levels can only be within 1 and 100001. Decimal numbers will not work");

        spoofPlatform = Config.Bind("MalumMenu.Spoofing",
                                "Platform",
                                "",
                                "A custom gaming platform to display to others in online lobbies to hide your actual platform. List of supported platforms: https://skeld.js.org/enums/_skeldjs_constant.Platform.html");

        spoofDeviceId = Config.Bind("MalumMenu.Privacy",
                                "HideDeviceId",
                                true,
                                "When enabled, it will hide your unique deviceId from Among Us, which could potentially help bypass hardware bans in the future");

        noTelemetry = Config.Bind("MalumMenu.Privacy",
                                "NoTelemetry",
                                true,
                                "When enabled, it will stop Among Us from collecting analytics of your games and sending them to Innersloth using Unity Analytics");

        // adaptMaxStrength = Config.Bind("MalumMenu.Overload",
        //                         "AdaptMaxStrength",
        //                         18000,
        //                         new ConfigDescription(
        //                             "Maximum total number of RPCs sent during one overload cycle in AutoAdapt mode. Automatically divided between targets and reduced based on ping. IMPORTANT: Only goes from 1 to 100K RPCs",
        //                             new AcceptableValueRange<int>(1, 100000)
        //                         ));

        // adaptMaxCooldown = Config.Bind("MalumMenu.Overload",
        //                         "AdaptMaxCooldown",
        //                         1f,
        //                         new ConfigDescription(
        //                             "Maximum time (in seconds) for one full overload cycle to complete in AutoAdapt mode. Automatically distributed across targets (more targets = shorter delay per target). IMPORTANT: Only goes from 0s to 10s",
        //                             new AcceptableValueRange<float>(0f, 10f)
        //                         ));

        // attackLogDelay = Config.Bind("MalumMenu.Overload",
        //                         "AttackLogDelay",
        //                         2f,
        //                         "Minimum time (in seconds) between attack logs in normal (non-verbose) mode");

        // defaultStrength = Config.Bind("MalumMenu.Overload",
        //                         "DefaultStrength",
        //                         18000,
        //                         new ConfigDescription(
        //                             "Default number of malformed RPCs sent to each target during an overload cycle. Overridden if AutoAdapt mode is enabled. IMPORTANT: Only goes from 1 to 100K RPCs",
        //                             new AcceptableValueRange<int>(1, 100000)
        //                         ));

        // defaultCooldown = Config.Bind("MalumMenu.Overload",
        //                         "DefaultCooldown",
        //                         1f,
        //                         new ConfigDescription(
        //                             "Default cooldown (in seconds) between each target during an overload cycle. Overridden if AutoAdapt mode is enabled. IMPORTANT: Only goes from 0s to 10s",
        //                             new AcceptableValueRange<float>(0f, 10f)
        //                         ));

        // killSwitchLvl = Config.Bind("MalumMenu.Overload",
        //                         "DefaultKillSwitchLevel",
        //                         1,
        //                         new ConfigDescription(
        //                             "Default level used by kill switch. Each level adds 500 ms to the max allowed ping before overload stops. Helps avoid lagging / disconnects. IMPORTANT: Only goes from level 1 (500 ms) to 6 (3000 ms)",
        //                             new AcceptableValueRange<int>(1, 6)
        //                         ));

        // Enabled by default
        CheatToggles.unlockFeatures = true;
        CheatToggles.freeCosmetics = true;
        CheatToggles.avoidPenalties = true;

        // Enabled by default
        CheatToggles.olAutoAdapt = true;
        CheatToggles.olKillSwitch = true;
        CheatToggles.olAutoStop = true;
        CheatToggles.olAutoClear = true;
        CheatToggles.olLogStartStop = true;
        CheatToggles.olLogAttack = true;
        CheatToggles.olLogAddRemove = true;
        CheatToggles.olLogDisconnect = true;

        Harmony.PatchAll();

        // UI
        //menuUI = AddComponent<MenuUI>();
        consoleUI = AddComponent<ConsoleUI>();
        doorsUI = AddComponent<DoorsUI>();
        tasksUI = AddComponent<TasksUI>();
        protectUI = AddComponent<ProtectUI>();
        // overloadUI = AddComponent<OverloadUI>();
        // rolesUI = AddComponent<RolesUI>();

        // Components
        keybindListener = AddComponent<KeybindListener>();

        // Disables Telemetry (haven't fully tested if it works, but according to Unity docs it should)
        if (noTelemetry.Value)
        {
            Analytics.enabled = false;
            Analytics.deviceStatsEnabled = false;
            PerformanceReporting.enabled = false;
        }

        // Create profile file if it is missing
        if (!File.Exists(ProfilePath))
        {
            CheatToggles.SaveTogglesToProfile();
        }

        // Auto load profile on start if needed
        if (autoLoadProfile.Value)
        {
            CheatToggles.LoadTogglesFromProfile();
        }

        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, _) =>
        {
            if (scene.name == "MainMenu" && !(inStealthMode || isPanicked))
            {
                // Warns about unsupported AU versions
                if (!supportedAU.Contains(Application.version))
                {
                    Utils.ShowPopup("\nThis version of MalumMenu and this version of Among Us are incompatible\n\nInstall the right version to avoid problems");
                }
            }
        }));

        float startupDelay = 1f;
        UniverseLib.Config.UniverseLibConfig config = new() {};
        Universe.Init(startupDelay, OnInitialized, LogHandler, config);
    }

    void OnInitialized()
    {
        UIBase = UniversalUI.RegisterUI("com.scp222thj.MalumMenu", UiUpdate);
        MainUIPanel mainUIPanel = new(UIBase);
    }

    void LogHandler(string message, LogType log) { }

    void UiUpdate() { }

    public class MovementTab : UIModel
    {
        public MainUIPanel Parent { get; }

        public MovementTab(MainUIPanel parent)
        {
            Parent = parent;
        }

        public override GameObject UIRoot => uiRoot;
        private GameObject uiRoot;

        public override void ConstructUI(GameObject content)
        {
            uiRoot = UIFactory.CreateUIObject("MovementTab", content);
        }

        public void Update(){}
    }

    public class ESPTab : UIModel
    {
        public MainUIPanel Parent { get; }

        public ESPTab(MainUIPanel parent)
        {
            Parent = parent;
        }

        public override GameObject UIRoot => uiRoot;
        private GameObject uiRoot;

        public override void ConstructUI(GameObject parent)
        {
            //uiRoot = UIFactory.CreateVerticalGroup(parent, "ObjectSearch", true, true, true, true, 2, new Vector4(2, 2, 2, 2));
            //UIFactory.SetLayoutElement(uiRoot, flexibleHeight: 9999);
        }

        public void Update(){}
    }

    public class MainUIPanel : UniverseLib.UI.Panels.PanelBase
    {
        public MainUIPanel(UIBase owner) : base(owner) { }

        public override string Name => $"MalumMenu v{malumVersion} by scp222thj & Astral";
        public override int MinWidth => 100;
        public override int MinHeight => 200;
        public override Vector2 DefaultAnchorMin => new(0.25f, 0.25f);
        public override Vector2 DefaultAnchorMax => new(0.75f, 0.75f);
        public override bool CanDragAndResize => true;

        private readonly List<UIModel> tabPages = new();
        private readonly List<ButtonRef> tabButtons = new();
        public int SelectedTab = 0;

        public MovementTab MovementTab;
        public ESPTab ESPTab;

        protected override void ConstructPanelContent()
        {
            GameObject tabGroup = UIFactory.CreateVerticalGroup(ContentRoot, "TabBar", false, false, true, true, 2,
                new Vector4(2, 2, 2, 2));
            UIFactory.SetLayoutElement(tabGroup, minHeight: 25, flexibleHeight: 0);

            ButtonRef movementTabButton = UIFactory.CreateButton(tabGroup, $"Button_Movement", "Movement", new Color(30, 30, 42));
            UIFactory.SetLayoutElement(movementTabButton.Component.gameObject, minHeight: 23, flexibleHeight: 0, minWidth: 100);

            ButtonRef espTabButton = UIFactory.CreateButton(tabGroup, $"Button_ESP", "ESP");
            UIFactory.SetLayoutElement(espTabButton.Component.gameObject, minHeight: 23, flexibleHeight: 0, minWidth: 100);

            ButtonRef rolesTabButton = UIFactory.CreateButton(tabGroup, $"Button_Roles", "Roles");
            UIFactory.SetLayoutElement(rolesTabButton.Component.gameObject, minHeight: 23, flexibleHeight: 0, minWidth: 100);
/*
            MovementTab = new MovementTab(this);
            MovementTab.ConstructUI(ContentRoot);
            tabPages.Add(MovementTab);

            ESPTab = new ESPTab(this);
            ESPTab.ConstructUI(ContentRoot);
            tabPages.Add(ESPTab);

            AddTabButton(tabGroup, "Host-Only");
            AddTabButton(tabGroup, "Modes");
            AddTabButton(tabGroup, "Config");*/

            Text myText = UIFactory.CreateLabel(ContentRoot, "myText", "Hello world");

            InputFieldRef searchField = UIFactory.CreateInputField(ContentRoot, "searchField", "Search...");
            UIFactory.SetLayoutElement(searchField.Component.gameObject, minWidth: 200, minHeight: 25);

            UIFactory.CreateSlider(ContentRoot, "mySlider", out var mySlider);
            mySlider.m_MaxValue = 100f;
            mySlider.m_MinValue = 0f;

            UIFactory.CreateToggle(ContentRoot, "myToggle", out var myToggle, out var myText2);
            myText2.text = "hi uwu~";

            UIFactory.SetLayoutElement(myText.gameObject, minWidth: 200, minHeight: 25);
        }

        private void DisableTab(int tabIndex)
        {
            tabPages[tabIndex].SetActive(false);
            RuntimeHelper.SetColorBlock(tabButtons[tabIndex].Component, UniversalUI.DisabledButtonColor, UniversalUI.DisabledButtonColor * 1.2f);
        }

        public void SetTab(int tabIndex)
        {
            if (SelectedTab != -1)
                DisableTab(SelectedTab);

            UIModel content = tabPages[tabIndex];
            content.SetActive(true);

            ButtonRef button = tabButtons[tabIndex];
            RuntimeHelper.SetColorBlock(button.Component, UniversalUI.EnabledButtonColor, UniversalUI.EnabledButtonColor * 1.2f);

            SelectedTab = tabIndex;
            //SaveInternalData();
        }

        void AddTabButton(GameObject tabGroup, string label)
        {
            ButtonRef button = UIFactory.CreateButton(tabGroup, $"Button_{label}", label);

            int idx = tabButtons.Count;
            //button.onClick.AddListener(() => { SetTab(idx); });
            button.OnClick += () => { SetTab(idx); };

            tabButtons.Add(button);

            DisableTab(tabButtons.Count - 1);
        }

        /*public override void Update()
        {
            if (SelectedTab == 0)
                MovementTab.Update();
            else
                ESPTab.Update();
        }*/

        // override other methods as desired
    }
}
