﻿using System;
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
        int winId, selWinId;
        double quickHideEnd = 0;

        public const float SEL_WINDOW_WIDTH = 600;
        public const float SEL_WINDOW_HEIGHT = 200;

        internal const string MODID = "DisplayCurrentContract";
        internal const string MODNAME = "Display Current Contract";

        Rect selWinPos = new Rect(Screen.width / 2 - SEL_WINDOW_WIDTH / 2, Screen.height / 2 - SEL_WINDOW_HEIGHT / 2, SEL_WINDOW_WIDTH, SEL_WINDOW_HEIGHT);

        int numDisplayedContracts = 0;
        string contractText = "Contracts";

        bool resizingWindow = false;

        internal class Contract
        {
            internal bool selected;
            internal bool active;
            internal contractContainer contractContainer;

            internal Contract(contractContainer cc)
            {
                selected = false;
                active = true;
                contractContainer = cc;
            }
        }
        Dictionary<Guid, Contract> activeContracts;
        Vector2 scrollPos;

        public void Start()
        {
            GameEvents.onHideUI.Add(OnHideUI);
            GameEvents.onShowUI.Add(OnShowUI);
            winId = WindowHelper.NextWindowId("SimpleContractDisplay");
            selWinId = WindowHelper.NextWindowId("CCD_Select");

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
            InvokeRepeating("Slowupdate", 5, 5);
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
        }


        void Slowupdate()
        {
            if (activeContracts == null)
                return;
            foreach (var a in activeContracts)
                a.Value.active = false;
            var aContracts = contractParser.getActiveContracts;
            int cnt = 0;
            foreach (var a in aContracts)
            {
                if (activeContracts.ContainsKey(a.ID))
                {
                    activeContracts[a.ID].active = true;
                    cnt++;
                }

            }
            while (activeContracts.Count - cnt > 0)
            {
                foreach (var a in activeContracts)
                {
                    if (!a.Value.active)
                    {
                        Log.Info("Deleting from activeContracts: " + a.Value.contractContainer.ID);
                        activeContracts.Remove(a.Value.contractContainer.ID);
                        cnt++;
                        break;
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

        Vector2 contractPos;
        Dictionary<string, bool> openClosed = new Dictionary<string, bool>();

        void ContractWindowDisplay(int id)
        {
            numDisplayedContracts = 0;

            if (activeContracts != null)
            {
                contractPos = GUILayout.BeginScrollView(contractPos, GUILayout.MaxHeight(Screen.height - 20));
                foreach (var a in activeContracts)
                {

                    if (a.Value.selected)
                    {
                        numDisplayedContracts++;

                        string contractId = a.Value.contractContainer.ID.ToString();
                        bool requirementsOpen = false;
                        if (!openClosed.TryGetValue(contractId, out requirementsOpen))
                            openClosed.Add(contractId, requirementsOpen);

                        using (new GUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button(requirementsOpen ? "-" : "+", GUILayout.Width(20)))
                            {
                                requirementsOpen = !requirementsOpen;
                                openClosed[contractId] = requirementsOpen;
                            }
                            GUILayout.TextField(a.Value.contractContainer.Title, Settings.Instance.displayFont);
                        }
                        if (Settings.Instance.showBriefing)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(20);
                                GUILayout.TextArea(a.Value.contractContainer.Briefing, Settings.Instance.textAreaFont);
                            }
                        }
                        if (!String.IsNullOrWhiteSpace(a.Value.contractContainer.Notes) && Settings.Instance.showNotes && requirementsOpen)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.Space(40);
                                GUILayout.TextArea("<color=#acfcff>" + a.Value.contractContainer.Notes + "</color>", Settings.Instance.textAreaSmallFont);
                            }
                        }
                        int paramCnt = 0;
                        if (requirementsOpen)
                        {
                            foreach (ContractParser.parameterContainer p in a.Value.contractContainer.ParamList)
                            {
                                paramCnt++;
                                if (Settings.Instance.showRequirements)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Space(40);
                                        GUILayout.TextArea(p.Title, Settings.Instance.textAreaFont);
                                    }
                                }
                                if (!String.IsNullOrWhiteSpace(p.Notes()) && Settings.Instance.showNotes)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Space(40);
                                        GUILayout.TextArea("<color=#acfcff>" + p.Notes(true) + "</color>", Settings.Instance.textAreaSmallFont);
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

                        if (activeContracts == null)
                            activeContracts = new Dictionary<Guid, Contract>();

                        var aContracts = contractParser.getActiveContracts;
                        foreach (var a in aContracts)
                        {
                            if (!activeContracts.ContainsKey(a.ID))
                                activeContracts.Add(a.ID, new Contract(a));
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
                quickHideEnd = Time.realtimeSinceStartup + 15;
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
            Settings.Instance.textAreaFont.fontSize = (int)fontSize;
            Settings.Instance.textAreaFont.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;

            Settings.Instance.textAreaSmallFont.fontSize = (int)fontSize - 2;
            Settings.Instance.textAreaSmallFont.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            Settings.Instance.textAreaSmallFont.richText = true;
        }

        internal static void SetAlpha(float Alpha)
        {
            GUIStyle workingWindow;

            if (Settings.Instance.kspWindow.active.background == null)
            {
                Log.Info("SetAlpha, Settings.Instance.kspWindow.active.background is null");
                Settings.Instance.kspWindow.active.background = GUISkinCopy.CopyTexture2D(HighLogic.Skin.window.active.background);
            }

            //if (useStockSkin)
            workingWindow = Settings.Instance.kspWindow;
            // else
            //     workingWindow = Settings.Instance.unityWindow;


            Texture2D copyTexture = GUISkinCopy.CopyTexture2D(HighLogic.Skin.window.active.background);

            var pixels = copyTexture.GetPixels32();
            for (int i = 0; i < pixels.Length; ++i)
                pixels[i].a = (byte)Alpha;


            copyTexture.SetPixels32(pixels);
            copyTexture.Apply();

            workingWindow.active.background =
                workingWindow.normal.background =
                workingWindow.hover.background =
                workingWindow.onNormal.background =
                workingWindow.onHover.background =
                workingWindow.onActive.background =
                workingWindow.focused.background =
                workingWindow.onFocused.background =
                workingWindow.onNormal.background =
                workingWindow.normal.background = copyTexture;

            workingWindow.active.textColor =
                workingWindow.normal.textColor =
                workingWindow.hover.textColor =
                workingWindow.onNormal.textColor =
                workingWindow.onHover.textColor =
                workingWindow.onActive.textColor =
                workingWindow.focused.textColor =
                workingWindow.onFocused.textColor =
                workingWindow.onNormal.textColor =
                workingWindow.normal.textColor = workingWindow.active.textColor;
        }


        void SelectContractWindowDisplay(int id)
        {
            using (new GUILayout.VerticalScope())
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                foreach (var a in activeContracts.Values)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        a.selected = GUILayout.Toggle(a.selected, "");
                        if (GUILayout.Button(a.contractContainer.Title))
                            a.selected = !a.selected;
                    }
                }
                GUILayout.EndScrollView();
                if (GUILayout.Button("Accept"))
                {
                    selectVisible = false;
                    WriteContractsToFile();
                    Settings.Instance.ResetWinPos();
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
                if (activeContracts != null)
                {
                    foreach (var a in activeContracts)
                    {
                        if (a.Value.selected)
                        {
                            str.AppendLine(a.Value.contractContainer.Title);
                            str.AppendLine(a.Value.contractContainer.Briefing);
                            str.AppendLine();
                        }
                    }
                    try
                    {
                        File.WriteAllText(Settings.Instance.fileName, str.ToString());
                    }
                    catch (Exception ex)
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
