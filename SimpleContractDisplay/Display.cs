using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using ClickThroughFix;
using ToolbarControl_NS;
using SpaceTuxUtility;
using ContractParser;


using static SimpleContractDisplay.RegisterToolbar;

// Transparency for unity skin ???

namespace SimpleContractDisplay
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, true)]
    public class Display : MonoBehaviour
    {
        private static ToolbarControl toolbarControl;
        internal static ToolbarControl Toolbar { get { return toolbarControl; } }

        bool hide = false;
        void OnHideUI() { hide = true; }
        void OnShowUI() { hide = false; }
        bool Hide { get { return hide; } }

        bool visible = false;
        bool selectVisible = false;
        bool settingsVisible = false;
        int winId, selWinId, manualContractWinId;
        double quickHideEnd = 0;
        string manualContract = "";
        string manualTitle = "";
        bool displayManualContractEntry = false;

        public const float SEL_WINDOW_WIDTH = 600;
        public const float SEL_WINDOW_HEIGHT = 200;

        internal const string MODID = "DisplayCurrentContract";
        internal const string MODNAME = "Display Current Contract";

        Rect selWinPos = new Rect(Screen.width / 2 - SEL_WINDOW_WIDTH / 2, Screen.height / 2 - SEL_WINDOW_HEIGHT / 2, SEL_WINDOW_WIDTH, SEL_WINDOW_HEIGHT);
        Rect manualContractWinPos = new Rect(Screen.width / 2 - SEL_WINDOW_WIDTH / 2, Screen.height / 2 - SEL_WINDOW_HEIGHT / 2, SEL_WINDOW_WIDTH, SEL_WINDOW_HEIGHT);

        int numDisplayedContracts = 0;
        string contractText = "Contracts";

        bool resizingWindow = false;

        Vector2 scrollPos;

        public void Start()
        {
            GameEvents.onHideUI.Add(OnHideUI);
            GameEvents.onShowUI.Add(OnShowUI);
            winId = WindowHelper.NextWindowId("SimpleContractDisplay");
            selWinId = WindowHelper.NextWindowId("CCD_Select");
            manualContractWinId = WindowHelper.NextWindowId("ManualContractEntry");
            DontDestroyOnLoad(this);

            if (toolbarControl == null)
            {
                toolbarControl = gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(GUIToggle, GUIToggle,
                     ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.TRACKSTATION | ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.MAPVIEW,
                     MODID,
                     "CCD_Btn",
                     "SimpleContractDisplay/PluginData/textures/contract-38.png",
                     "SimpleContractDisplay/PluginData/textures/contract-24.png",
                    MODNAME);
            }
            InvokeRepeating("SlowUpdate", 5, 5);
            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);
        }

        public void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> eData)
        {
            Settings.Instance.SaveData();

            switch (eData.from)
            {
                case GameScenes.EDITOR:
                    Settings.Instance.editorWinPos = new Rect(Settings.Instance.winPos);
                    break;
                case GameScenes.FLIGHT:
                    Settings.Instance.flightWinPos = new Rect(Settings.Instance.winPos);
                    break;
                case GameScenes.SPACECENTER:
                    Settings.Instance.spaceCenterWinPos = new Rect(Settings.Instance.winPos);
                    break;
                case GameScenes.TRACKSTATION:
                    Settings.Instance.trackStationWinPos = new Rect(Settings.Instance.winPos);
                    break;
            }
            switch (eData.to)
            {
                case GameScenes.EDITOR:
                    if (Settings.Instance.editorWinPos.width == 0)
                        Settings.Instance.editorWinPos = new Rect(Settings.Instance.winPos);
                    else
                        Settings.Instance.winPos = new Rect(Settings.Instance.editorWinPos);
                    break;
                case GameScenes.FLIGHT:
                    if (Settings.Instance.flightWinPos.width == 0)
                        Settings.Instance.flightWinPos = new Rect(Settings.Instance.winPos);
                    else
                        Settings.Instance.winPos = new Rect(Settings.Instance.flightWinPos);
                    break;
                case GameScenes.SPACECENTER:
                    if (Settings.Instance.spaceCenterWinPos.width == 0)
                        Settings.Instance.spaceCenterWinPos = new Rect(Settings.Instance.winPos);
                    else
                        Settings.Instance.winPos = new Rect(Settings.Instance.spaceCenterWinPos);
                    break;
                case GameScenes.TRACKSTATION:
                    if (Settings.Instance.trackStationWinPos.width == 0)
                        Settings.Instance.trackStationWinPos = new Rect(Settings.Instance.winPos);
                    else
                        Settings.Instance.winPos = new Rect(Settings.Instance.trackStationWinPos);
                    break;
            }
            Settings.Instance.winPos.width = Math.Max(Settings.Instance.winPos.width, Settings.WINDOW_WIDTH);
            if (HighLogic.CurrentGame != null)
            {
#if false
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
                {
                    toolbarControl.buttonActive = true;
                    toolbarControl.Enabled = true;
                }
                else
                {
                    toolbarControl.buttonActive = false;
                    toolbarControl.Enabled = false;
                }
#endif
            }
        }


        void SlowUpdate()
        {
            if (Settings.Instance.activeContracts == null)
                return;
            foreach (var a in Settings.Instance.activeContracts)
                a.Value.active = false;
            var aContracts = contractParser.getActiveContracts;
            int cnt = 0;
            foreach (var a in aContracts)
            {
                if (Settings.Instance.activeContracts.ContainsKey(a.ID))
                {
                    Settings.Instance.activeContracts[a.ID].active = true;
                    cnt++;
                }

            }
            while (Settings.Instance.activeContracts.Count - cnt > 0)
            {
                foreach (var a in Settings.Instance.activeContracts)
                {
                    if (a.Value.manual)
                        cnt++;
                    else
                    {
                        if (!a.Value.active)
                        {
                            Log.Info("Deleting from activeContracts: " + a.Key);
                            Settings.Instance.activeContracts.Remove(a.Key);
                            cnt++;
                            break;
                        }
                    }
                }
            }
            WriteContractsToFile();
        }

        private void GUIToggle()
        {
            visible = !visible;
        }

        public void OnGUI()
        {
            if (HighLogic.LoadedSceneIsGame && (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX) &&
                !Hide && visible && Settings.Instance != null && Time.realtimeSinceStartup > quickHideEnd)
            {
                Rect tmpPos;
                GUI.skin = HighLogic.Skin;
                SetAlpha(Settings.Instance.Alpha);

                if (displayManualContractEntry)
                    manualContractWinPos = ClickThruBlocker.GUILayoutWindow(manualContractWinId, manualContractWinPos, EnterManualContract, "Manual Contract Entry");
                else
                {
                    if (!selectVisible)
                    {
                        if (Settings.Instance.enableClickThrough && !settingsVisible)
                            tmpPos = GUILayout.Window(winId, Settings.Instance.winPos, ContractWindowDisplay, "Simple Contract Display - Active " + contractText, Settings.Instance.kspWindow);
                        else
                            tmpPos = ClickThruBlocker.GUILayoutWindow(winId, Settings.Instance.winPos, ContractWindowDisplay, "Simple Contract Display - Active " + contractText + " & Settings", Settings.Instance.kspWindow);
                        if (!Settings.Instance.lockPos)
                            Settings.Instance.winPos = tmpPos;
                    }
                    else
                    {
                        selWinPos = ClickThruBlocker.GUILayoutWindow(selWinId, selWinPos, SelectContractWindowDisplay, "Simple Contract Display - Contract Selection");
                    }
                }
            }
        }


        void RecurseParameterContainer(float indent, parameterContainer p)
        {
            for (int i2 = 0; i2 < p.ParamList.Count; i2++)
            {
                parameterContainer p1 = p.ParamList[i2];
                if (!String.IsNullOrWhiteSpace(p1.Title))
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(indent);
                        GUILayout.TextArea("<color=#acfcff>" + p1.Title + "</color>", Settings.Instance.textAreaSmallFont);
                    }
                if (!String.IsNullOrWhiteSpace(p.CParam.Notes))
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(indent);
                        GUILayout.TextArea("<color=#acfcff>" + p1.CParam.Notes + "</color>", Settings.Instance.textAreaSmallFont);
                    }

                for (int i3 = 0; i3 < p.ParamList.Count; i3++)
                    RecurseParameterContainer(indent + 10, p.ParamList[i3]);
            }
        }

        Vector2 contractPos;
        Dictionary<string, bool> openClosed = new Dictionary<string, bool>();

        void ContractWindowDisplay(int id)
        {
            numDisplayedContracts = 0;

            if (Settings.Instance.activeContracts != null)
            {
                contractPos = GUILayout.BeginScrollView(contractPos, Settings.Instance.scrollViewStyle, GUILayout.MaxHeight(Screen.height - 20));
                foreach (var contract in Settings.Instance.activeContracts)
                {
                    if (contract.Value.selected)
                    {
                        numDisplayedContracts++;

                        string contractId = contract.Key.ToString();
                        bool requirementsOpen = false;
                        if (!contract.Value.manual)
                        {
                            if (!openClosed.TryGetValue(contractId, out requirementsOpen))
                                openClosed.Add(contractId, requirementsOpen);
                        }
                        using (new GUILayout.HorizontalScope())
                        {
                            if (!contract.Value.manual)
                            {
                                if (GUILayout.Button(requirementsOpen ? "-" : "+", GUILayout.Width(20)))
                                {
                                    requirementsOpen = !requirementsOpen;
                                    openClosed[contractId] = requirementsOpen;
                                }
                            }
                            else
                                GUILayout.Space(22);
                            GUILayout.TextField(contract.Value.manual ? contract.Value.manualTitle : contract.Value.contractContainer.Title, Settings.Instance.displayFont);
                        }

                        if (Settings.Instance.showBriefing)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(20);
                                GUILayout.TextArea(contract.Value.manual ? contract.Value.manualContract : contract.Value.contractContainer.Briefing, Settings.Instance.textAreaFont);
                            }
                        }
                        if (!contract.Value.manual)
                        {
                            if (requirementsOpen)
                            {
                                if (!String.IsNullOrWhiteSpace(contract.Value.contractContainer.Notes) && Settings.Instance.showNotes)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Space(40);
                                        GUILayout.TextArea("<color=#acfcff>" + contract.Value.contractContainer.Notes + "</color>", Settings.Instance.textAreaSmallFont);
                                    }
                                }

                            }
                            int paramCnt = 0;
                            if (requirementsOpen)
                            {
                                for (int i1 = 0; i1 < contract.Value.contractContainer.ParamList.Count; i1++)
                                {
                                    parameterContainer p = contract.Value.contractContainer.ParamList[i1];

                                    paramCnt++;
                                    if (Settings.Instance.showRequirements)
                                    {
                                        if (!String.IsNullOrWhiteSpace(p.Title))
                                        {
                                            using (new GUILayout.HorizontalScope())
                                            {
                                                GUILayout.Space(40);
                                                GUILayout.TextArea(p.Title, Settings.Instance.textAreaFont);
                                            }
                                        }
                                    }
                                    if (Settings.Instance.showNotes)
                                    {
                                        if (!String.IsNullOrWhiteSpace(p.Notes()))
                                        {
                                            using (new GUILayout.HorizontalScope())
                                            {
                                                GUILayout.Space(40);
                                                GUILayout.TextArea("<color=#acfcff>" + p.Notes(true) + "</color>", Settings.Instance.textAreaSmallFont);
                                            }
                                        }
                                        if (!String.IsNullOrWhiteSpace(p.CParam.Notes))
                                            using (new GUILayout.HorizontalScope())
                                            {
                                                GUILayout.Space(60);
                                                GUILayout.TextArea("<color=#acfcff>" + p.CParam.Notes + "</color>", Settings.Instance.textAreaSmallFont);
                                            }
                                        RecurseParameterContainer(60, p);
                                    }
                                }
                            }
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            contractText = (numDisplayedContracts != 1) ? "Contracts" : "Contract";

            if (settingsVisible)
            {
                bool oBold = Settings.Instance.bold;
                var oAlpha = Settings.Instance.Alpha;

                using (new GUILayout.HorizontalScope())
                {
                    // This stupidity is due to a bug in the KSP skin
                    Settings.Instance.showBriefing = GUILayout.Toggle(Settings.Instance.showBriefing, "");
                    GUILayout.Label("Display Briefing");
                    Settings.Instance.bold = GUILayout.Toggle(Settings.Instance.bold, "");
                    GUILayout.Label("Bold");
                    GUILayout.FlexibleSpace();
                    Settings.Instance.lockPos = GUILayout.Toggle(Settings.Instance.lockPos, "");
                    GUILayout.Label("Lock Position");
                    Settings.Instance.hideButtons = GUILayout.Toggle(Settings.Instance.hideButtons, "");
                    GUILayout.Label("Hide Buttons");
                }
                using (new GUILayout.HorizontalScope())
                {
                    Settings.Instance.enableClickThrough = GUILayout.Toggle(Settings.Instance.enableClickThrough, "");
                    GUILayout.Label("Allow click-through");
                    Settings.Instance.showRequirements = GUILayout.Toggle(Settings.Instance.showRequirements, "");
                    GUILayout.Label("Display Requirements");
                    Settings.Instance.showNotes = GUILayout.Toggle(Settings.Instance.showNotes, "");
                    GUILayout.Label("Display Notes");
                }
                using (new GUILayout.HorizontalScope())
                {
                    Settings.Instance.saveToFile = GUILayout.Toggle(Settings.Instance.saveToFile, "");
                    GUILayout.Label("Save to file");
                    if (Settings.Instance.saveToFile)
                    {
                        bool exists = false;
                        if (Settings.Instance.fileName.Length > 0)
                            exists = Directory.Exists(Path.GetDirectoryName(Settings.Instance.fileName)) || Path.GetDirectoryName(Settings.Instance.fileName) == "";
                        GUILayout.Space(20);
                        Settings.Instance.fileName = GUILayout.TextField(Settings.Instance.fileName,
                           exists ? Settings.Instance.textFieldStyleNormal : Settings.Instance.textFieldStyleRed,
                           GUILayout.MinWidth(60), GUILayout.ExpandWidth(true));
                    }
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Transparency:", GUILayout.Width(130));
                    Settings.Instance.Alpha = GUILayout.HorizontalSlider(Settings.Instance.Alpha, 0f, 255f, GUILayout.Width(130));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Hide Time (" + Settings.Instance.HideTime.ToString("F0") + "s):");
                    Settings.Instance.HideTime = GUILayout.HorizontalSlider(Settings.Instance.HideTime, 1f, 30, GUILayout.Width(130));
                    GUILayout.FlexibleSpace();
                }
                if (oAlpha != Settings.Instance.Alpha)
                {
                    SetAlpha(Settings.Instance.Alpha);
                }
                var oFontSize = Settings.Instance.fontSize;
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Font Size:", GUILayout.Width(130));
                    Settings.Instance.fontSize = GUILayout.HorizontalSlider(Settings.Instance.fontSize, 8f, 30f, GUILayout.Width(130));
                    GUILayout.FlexibleSpace();
                }
                if (oFontSize != Settings.Instance.fontSize || oBold != Settings.Instance.bold)
                    SetFontSizes(Settings.Instance.fontSize, Settings.Instance.bold);
            }

            if (!Settings.Instance.hideButtons || settingsVisible || numDisplayedContracts == 0)
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Select"))
                    {
                        selectVisible = true;

                        if (Settings.Instance.activeContracts == null)
                            Settings.Instance.activeContracts = new Dictionary<Guid, Contract>();

                        var aContracts = contractParser.getActiveContracts;
                        foreach (var a in aContracts)
                        {
                            if (!Settings.Instance.activeContracts.ContainsKey(a.ID))
                                Settings.Instance.activeContracts.Add(a.ID, new Contract(a));
                        }
                    }
                    if (GUILayout.Button("Close"))
                    {
                        GUIToggle();
                    }

                    if (GUILayout.Button(settingsVisible ? "Close Settings" : "Settings"))
                    {
                        settingsVisible = !settingsVisible;
                        if (!settingsVisible)
                        {
                            Settings.Instance.SaveData();
                            Settings.Instance.failToWrite = false;
                        }
                    }
                    GUILayout.Space(30);
                }
            }
            if (GUI.Button(new Rect(4, 2, 16, 16), "B"))
            {
                Settings.Instance.hideButtons = !Settings.Instance.hideButtons;
            }
            if (GUI.Button(new Rect(22, 2, 16, 16), "L"))
            {
                Settings.Instance.lockPos = !Settings.Instance.lockPos;
            }
            if (GUI.Button(new Rect(40, 2, 16, 16), "R"))
            {
                Settings.Instance.showRequirements = !Settings.Instance.showRequirements;
            }
            if (GUI.Button(new Rect(58, 2, 16, 16), "N"))
            {
                Settings.Instance.showNotes = !Settings.Instance.showNotes;
            }
            if (GUI.Button(new Rect(76, 2, 16, 16), "S"))
            {
                settingsVisible = !settingsVisible;
            }

            if (GUI.Button(new Rect(Settings.Instance.winPos.width - 18, 2, 16, 16), "X"))
            {
                GUIToggle();
            }
            if (GUI.Button(new Rect(Settings.Instance.winPos.width - 36, 2, 16, 16), "H"))
            {
                quickHideEnd = Time.realtimeSinceStartup + Settings.Instance.HideTime;
            }
            if (!Settings.Instance.lockPos)
            {
                if (GUI.RepeatButton(new Rect(Settings.Instance.winPos.width - 23f, Settings.Instance.winPos.height - 23f, 16, 16), "", Settings.Instance.resizeButton))
                {
                    resizingWindow = true;
                }
            }
            resizeWindow();
            GUI.DragWindow();
        }

        private void resizeWindow()
        {
            if (Input.GetMouseButtonUp(0))
            {
                resizingWindow = false;
            }

            if (resizingWindow)
            {
                Settings.Instance.winPos.width = Input.mousePosition.x - Settings.Instance.winPos.x + 10;
                Settings.Instance.winPos.width = Mathf.Clamp(Settings.Instance.winPos.width, Settings.WINDOW_WIDTH, Screen.width);
                Settings.Instance.winPos.height = (Screen.height - Input.mousePosition.y) - Settings.Instance.winPos.y + 10;
                Settings.Instance.winPos.height = Mathf.Clamp(Settings.Instance.winPos.height, Settings.WINDOW_HEIGHT, Screen.height);
            }
        }

        static internal void SetFontSizes(float fontSize, bool bold)
        {
            Settings.Instance.displayFont.fontSize = (int)fontSize;
            Settings.Instance.displayFont.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            Settings.Instance.displayFont.normal.textColor = Color.yellow;

            Settings.Instance.textAreaFont.fontSize = (int)fontSize;
            Settings.Instance.textAreaFont.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            Settings.Instance.textAreaFont.normal.textColor = Color.white;

            Settings.Instance.textAreaSmallFont.fontSize = (int)fontSize - 2;
            Settings.Instance.textAreaSmallFont.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            Settings.Instance.textAreaSmallFont.richText = true;
            Settings.Instance.textAreaSmallFont.normal.textColor = Color.white;


        }

        static float lastAlpha = -1;
        internal static void SetAlpha(float Alpha)
        {
            GUIStyle workingWindow;
            if (Alpha == lastAlpha)
                return;
            lastAlpha = Alpha;
            if (Settings.Instance.kspWindow.active.background == null)
            {
                Log.Info("SetAlpha, Settings.Instance.kspWindow.active.background is null");
                Settings.Instance.kspWindow.active.background = GUISkinCopy.CopyTexture2D(HighLogic.Skin.window.active.background);
            }

            workingWindow = Settings.Instance.kspWindow;

            SetAlphaFor(Alpha, workingWindow, HighLogic.Skin.window.active.background, workingWindow.active.textColor);
            SetAlphaFor(Alpha, Settings.Instance.textAreaFont, HighLogic.Skin.textArea.normal.background, Settings.Instance.textAreaFont.normal.textColor);
            SetAlphaFor(Alpha, Settings.Instance.textAreaSmallFont, HighLogic.Skin.textArea.normal.background, Settings.Instance.textAreaSmallFont.normal.textColor);
            SetAlphaFor(Alpha, Settings.Instance.displayFont, HighLogic.Skin.textArea.normal.background, Settings.Instance.displayFont.normal.textColor);
            SetAlphaFor(Alpha, Settings.Instance.scrollViewStyle, HighLogic.Skin.scrollView.normal.background, workingWindow.active.textColor);
        }

        static void SetAlphaFor(float Alpha, GUIStyle style, Texture2D backgroundTexture, Color color)
        {
            Texture2D copyTexture = GUISkinCopy.CopyTexture2D(backgroundTexture);

            var pixels = copyTexture.GetPixels32();
            for (int i = 0; i < pixels.Length; ++i)
                pixels[i].a = (byte)Alpha;


            copyTexture.SetPixels32(pixels);
            copyTexture.Apply();

            style.active.background =
                style.normal.background =
                style.hover.background =
                style.onNormal.background =
                style.onHover.background =
                style.onActive.background =
                style.focused.background =
                style.onFocused.background =
                style.onNormal.background =
                style.normal.background = copyTexture;

            style.active.textColor =
                style.normal.textColor =
                style.hover.textColor =
                style.onNormal.textColor =
                style.onHover.textColor =
                style.onActive.textColor =
                style.focused.textColor =
                style.onFocused.textColor =
                style.onNormal.textColor =
                style.normal.textColor = color;
        }


        void SelectContractWindowDisplay(int id)
        {
            Guid? keyToRemove = null;
            using (new GUILayout.VerticalScope())
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos, Settings.Instance.scrollViewStyle);
                foreach (var a in Settings.Instance.activeContracts)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        a.Value.selected = GUILayout.Toggle(a.Value.selected, "");

                        if (GUILayout.Button(a.Value.manual ? a.Value.manualTitle : a.Value.contractContainer.Title))
                            a.Value.selected = !a.Value.selected;
                        if (a.Value.manual)
                        {
                            if (GUILayout.Button("Delete", GUILayout.Width(60)))
                                keyToRemove = a.Key;
                        }
                    }
                }
                GUILayout.EndScrollView();
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Accept"))
                    {
                        selectVisible = false;
                        WriteContractsToFile();
                        Settings.Instance.ResetWinPos();
                    }
                    if (GUILayout.Button("Manual Contract Entry", GUILayout.Width(180)))
                        displayManualContractEntry = true;
                }
            }
            if (keyToRemove != null)
                Settings.Instance.activeContracts.Remove((Guid)keyToRemove);
            GUI.DragWindow();
        }


        Vector2 contractScroll;
        void EnterManualContract(int id)
        {
            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Title:");
                    manualTitle = GUILayout.TextField(manualTitle);
                }
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("Descr:");
                    contractScroll = GUILayout.BeginScrollView(contractScroll, Settings.Instance.scrollViewStyle, GUILayout.MinHeight(100), GUILayout.MaxHeight(300));
                    manualContract = GUILayout.TextArea(manualContract, Settings.Instance.textAreaWordWrap, GUILayout.ExpandHeight(true));
                    GUILayout.EndScrollView();
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Clear", GUILayout.Width(90)))
                    {
                        manualTitle = "";
                        manualContract = "";
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Save & Close", GUILayout.Width(120)))
                    {
                        displayManualContractEntry = false;
                        if (manualContract != "")
                            Settings.Instance.activeContracts.Add(Guid.NewGuid(), new Contract(manualTitle, manualContract));
                        manualTitle = "";
                        manualContract = "";
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Close without Save", GUILayout.Width(150)))
                    {
                        displayManualContractEntry = false;
                    }
                    GUILayout.FlexibleSpace();
                }

            }
            GUI.DragWindow();
        }

        void WriteContractsToFile()
        {
            if (!Settings.Instance.saveToFile)
                return;
            bool exists = Directory.Exists(Path.GetDirectoryName(Settings.Instance.fileName)) || Path.GetDirectoryName(Settings.Instance.fileName) == "";
            StringBuilder str = new StringBuilder();
            if (exists)
            {
                if (Settings.Instance.activeContracts != null)
                {
                    foreach (var a in Settings.Instance.activeContracts)
                    {
                        if (a.Value.selected)
                        {
                            str.AppendLine(a.Value.manual ? a.Value.manualTitle : a.Value.contractContainer.Title);
                            str.AppendLine(a.Value.manual ? a.Value.manualContract : a.Value.contractContainer.Briefing);
                            str.AppendLine();
                        }
                    }
                    try
                    {
                        File.WriteAllText(Settings.Instance.fileName, str.ToString());
                    }
                    catch //(Exception ex)
                    {
                        if (!Settings.Instance.failToWrite)
                            ScreenMessages.PostScreenMessage("Unable to write contracts to file: " + Settings.Instance.fileName, 10f);
                        Settings.Instance.failToWrite = true;
                    }
                }
            }
        }
    }
}
