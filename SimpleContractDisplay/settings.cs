using UnityEngine;
using System.IO;
using ToolbarControl_NS;
using SpaceTuxUtility;
using static SimpleContractDisplay.RegisterToolbar;

namespace SimpleContractDisplay
{
    public class Settings
    {
        public const float WINDOW_WIDTH = 400;
        public const float WINDOW_HEIGHT = 100;

        public static Settings Instance;
        internal GUIStyle displayFont, textAreaFont, textAreaSmallFont;
        internal GUIStyle kspWindow;

        internal GUIStyle myStyle;
        internal Texture2D styleOff;
        internal Texture2D styleOn;
        internal GUIStyle resizeButton;

        internal string rootPath;

        internal GUIStyle textFieldStyleRed;
        internal GUIStyle textFieldStyleNormal;
        internal bool failToWrite = false;

        // Following are saved in a file
        internal float fontSize = 12f;
        internal bool bold = false;
        internal bool showBriefing = true;
        internal float Alpha = 255;
        internal bool lockPos = false;
        internal bool hideButtons = false;
        internal bool enableClickThrough = true;
        internal bool showRequirements = false;
        internal bool showNotes = false;
        internal string fileName = "";
        internal bool saveToFile = false;
        internal Rect winPos = new Rect(Screen.width / 2 - WINDOW_WIDTH / 2, Screen.height / 2 - WINDOW_HEIGHT / 2, WINDOW_WIDTH, WINDOW_HEIGHT);
        internal Rect spaceCenterWinPos;
        internal Rect editorWinPos;
        internal Rect flightWinPos;
        internal Rect trackStationWinPos;

        internal static readonly string CFG_PATH = "/GameData/SimpleContractDisplay/PluginData/";
        static readonly string CFG_FILE = CFG_PATH + "displayInfo.cfg";

        static readonly string NODENAME = "DISPLAYINFO";

        public void ResetWinPos()
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
                spaceCenterWinPos = new Rect();
            if (HighLogic.LoadedScene != GameScenes.EDITOR)
                editorWinPos = new Rect();
            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
                flightWinPos = new Rect();
            if (HighLogic.LoadedScene != GameScenes.TRACKSTATION)
                trackStationWinPos = new Rect();
        }
        public void SaveData()
        {
            string fullPath = rootPath + CFG_FILE;
            var configFile = new ConfigNode();
            var configFileNode = new ConfigNode(NODENAME);
            configFileNode.AddValue("fontSize", fontSize);
            configFileNode.AddValue("bold", bold);
            configFileNode.AddValue("showBriefing", showBriefing);
            configFileNode.AddValue("Alpha", Alpha);
            configFileNode.AddValue("lockPos", lockPos);
            configFileNode.AddValue("hideButtons", hideButtons);
            configFileNode.AddValue("enableClickThrough", enableClickThrough);
            configFileNode.AddValue("showRequirements", showRequirements);
            configFileNode.AddValue("showNotes", showNotes);


            if (fileName != null)
                configFileNode.AddValue("fileName", fileName);
            configFileNode.AddValue("saveToFile", saveToFile);

            configFileNode.AddValue("x", winPos.x);
            configFileNode.AddValue("y", winPos.y);
            configFileNode.AddValue("width", winPos.width);
            configFileNode.AddValue("height", winPos.height);

            configFileNode.AddValue("x", spaceCenterWinPos.x);
            configFileNode.AddValue("y", spaceCenterWinPos.y);
            configFileNode.AddValue("width", spaceCenterWinPos.width);
            configFileNode.AddValue("height", spaceCenterWinPos.height);

            configFileNode.AddValue("x", editorWinPos.x);
            configFileNode.AddValue("y", editorWinPos.y);
            configFileNode.AddValue("width", editorWinPos.width);
            configFileNode.AddValue("height", editorWinPos.height);

            configFileNode.AddValue("x", flightWinPos.x);
            configFileNode.AddValue("y", flightWinPos.y);
            configFileNode.AddValue("width", flightWinPos.width);
            configFileNode.AddValue("height", flightWinPos.height);

            configFileNode.AddValue("x", trackStationWinPos.x);
            configFileNode.AddValue("y", trackStationWinPos.y);
            configFileNode.AddValue("width", trackStationWinPos.width);
            configFileNode.AddValue("height", trackStationWinPos.height);

            configFile.AddNode(configFileNode);
            configFile.Save(fullPath);
        }

        public void LoadData()
        {
            string fullPath = rootPath + CFG_FILE;
            Log.Info("LoadData, fullpath: " + fullPath);
            if (File.Exists(fullPath))
            {
                Log.Info("file exists");
                var configFile = ConfigNode.Load(fullPath);
                if (configFile != null)
                {
                    Log.Info("configFile loaded");
                    var configFileNode = configFile.GetNode(NODENAME);
                    if (configFileNode != null)
                    {
                        Log.Info("configFileNode loaded");
                        fontSize = configFileNode.SafeLoad("fontSize", fontSize);
                        bold = configFileNode.SafeLoad("bold", bold);
                        showBriefing = configFileNode.SafeLoad("showBriefing", showBriefing);
                        Alpha = configFileNode.SafeLoad("Alpha", Alpha);
                        lockPos = configFileNode.SafeLoad("lockPos", lockPos);
                        hideButtons = configFileNode.SafeLoad("hideButtons", hideButtons);
                        enableClickThrough = configFileNode.SafeLoad("enableClickThrough", enableClickThrough);
                        showRequirements = configFileNode.SafeLoad("showRequirements", showRequirements);
                        showNotes = configFileNode.SafeLoad("showNotes", showNotes);


                        fileName = configFileNode.SafeLoad("fileName", fileName);
                        saveToFile = configFileNode.SafeLoad("saveToFile", saveToFile);

                        winPos.x = configFileNode.SafeLoad("x", winPos.x);
                        winPos.y = configFileNode.SafeLoad("y", winPos.y);
                        winPos.width = configFileNode.SafeLoad("width", winPos.width);
                        winPos.height = configFileNode.SafeLoad("height", winPos.height);

                        spaceCenterWinPos.x = configFileNode.SafeLoad("x", spaceCenterWinPos.x);
                        spaceCenterWinPos.y = configFileNode.SafeLoad("y", spaceCenterWinPos.y);
                        spaceCenterWinPos.width = configFileNode.SafeLoad("width", spaceCenterWinPos.width);
                        spaceCenterWinPos.height = configFileNode.SafeLoad("height", spaceCenterWinPos.height);

                        editorWinPos.x = configFileNode.SafeLoad("x", editorWinPos.x);
                        editorWinPos.y = configFileNode.SafeLoad("y", editorWinPos.y);
                        editorWinPos.width = configFileNode.SafeLoad("width", editorWinPos.width);
                        editorWinPos.height = configFileNode.SafeLoad("height", editorWinPos.height);

                        flightWinPos.x = configFileNode.SafeLoad("x", flightWinPos.x);
                        flightWinPos.y = configFileNode.SafeLoad("y", flightWinPos.y);
                        flightWinPos.width = configFileNode.SafeLoad("width", flightWinPos.width);
                        flightWinPos.height = configFileNode.SafeLoad("height", flightWinPos.height);

                        trackStationWinPos.x = configFileNode.SafeLoad("x", trackStationWinPos.x);
                        trackStationWinPos.y = configFileNode.SafeLoad("y", trackStationWinPos.y);
                        trackStationWinPos.width = configFileNode.SafeLoad("width", trackStationWinPos.width);
                        trackStationWinPos.height = configFileNode.SafeLoad("height", trackStationWinPos.height);

                    }
                }
            }
        }
    }
}