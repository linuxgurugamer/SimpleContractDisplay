using UnityEngine;
using ToolbarControl_NS;
using KSP_Log;

namespace SimpleContractDisplay
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        internal static Log Log = null;

        bool initted = false;
        void Awake()
        {
            if (Log == null)
#if DEBUG
                Log = new Log("DisplayCurrentContract", Log.LEVEL.INFO);
#else
                Log = new Log("DisplayCurrentContract", Log.LEVEL.ERROR);
#endif

            DontDestroyOnLoad(this);
            Settings.Instance = new Settings();

        }

        void Start()
        {
            ToolbarControl.RegisterMod(Display.MODID, Display.MODNAME);
            Settings.Instance.kspWindow = new GUIStyle(HighLogic.Skin.window);
            Settings.Instance.kspWindow.active.background = GUISkinCopy.CopyTexture2D(HighLogic.Skin.window.active.background);

            Settings.Instance.LoadData();
        }

        void OnGUI()
        {
            if (!initted)
            {
                initted = true;

                Settings.Instance.displayFont = new GUIStyle(HighLogic.Skin.textField);
                Settings.Instance.displayFont.normal.textColor = Color.yellow;
                Settings.Instance.labelFont = new GUIStyle(HighLogic.Skin.label);

                Settings.Instance.textAreaFont = new GUIStyle(HighLogic.Skin.textArea);
                Settings.Instance.textAreaSmallFont = new GUIStyle(HighLogic.Skin.textArea);

                Settings.Instance.textAreaWordWrap = new GUIStyle(HighLogic.Skin.textArea);
                Settings.Instance.textAreaWordWrap.wordWrap = true;

                Settings.Instance.myStyle = new GUIStyle();
                Settings.Instance.styleOff = new Texture2D(2, 2);
                Settings.Instance.styleOn = new Texture2D(2, 2);

                Settings.Instance.resizeButton = GetToggleButtonStyle("resize", 20, 20, true);

                Settings.Instance.fileName =  "ContractData.txt";

                Settings.Instance.textFieldStyleRed = new GUIStyle(HighLogic.Skin.textField)
                {
                    focused = { textColor = Color.red },
                    hover= { textColor = Color.red },
                    normal = { textColor = Color.red },
                    alignment = TextAnchor.MiddleLeft,
                };
                Settings.Instance.textFieldStyleNormal = new GUIStyle(HighLogic.Skin.textField)
                {
                    alignment = TextAnchor.MiddleLeft,
                };
                Settings.Instance.scrollViewStyle = new GUIStyle(HighLogic.Skin.scrollView);

                Display.SetFontSizes(Settings.Instance.fontSize, Settings.Instance.bold);
                Display.SetAlpha(Settings.Instance.Alpha);
            }
        }

        public GUIStyle GetToggleButtonStyle(string styleName, int width, int height, bool hover)
        {
            Log.Info("GetToggleButtonStyle, styleName: " + styleName);

            ToolbarControl.LoadImageFromFile(ref Settings.Instance.styleOff, "GameData/SimpleContractDisplay/PluginData/textures/" + styleName + "_off");
            ToolbarControl.LoadImageFromFile(ref Settings.Instance.styleOn, "GameData/SimpleContractDisplay/PluginData/textures/" + styleName + "_on");

            Settings.Instance.myStyle.name = styleName + "Button";
            Settings.Instance.myStyle.padding = new RectOffset() { left = 0, right = 0, top = 0, bottom = 0 };
            Settings.Instance.myStyle.border = new RectOffset() { left = 0, right = 0, top = 0, bottom = 0 };
            Settings.Instance.myStyle.margin = new RectOffset() { left = 0, right = 0, top = 2, bottom = 2 };
            Settings.Instance.myStyle.normal.background = Settings.Instance.styleOff;
            Settings.Instance.myStyle.onNormal.background = Settings.Instance.styleOn;
            if (hover)
            {
                Settings.Instance.myStyle.hover.background = Settings.Instance.styleOn;
            }
            Settings.Instance.myStyle.active.background = Settings.Instance.styleOn;
            Settings.Instance.myStyle.fixedWidth = width;
            Settings.Instance.myStyle.fixedHeight = height;
            return Settings.Instance.myStyle;
        }


    }
}