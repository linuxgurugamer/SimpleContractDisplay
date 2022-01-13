/**********************************************************************************************************/
/***                                              Import                                                ***/
/**********************************************************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using static SimpleContractDisplay.RegisterToolbar;

/**********************************************************************************************************/
/***                                    |          Class        |                                       ***/
/**********************************************************************************************************/
namespace SimpleContractDisplay
{
    public class GUISkinCopy : MonoBehaviour
    {

#if false
		public static GUISkin DoCopyGUISkin(GUISkin guiskin)
		{
			GUISkin skin = guiskin;
			GUISkin newSkin = new  GUISkin();

			Log.Info("DoCopyGUISkin 1");
			foreach (PropertyInfo propertyInfo in typeof(GUISkin).GetProperties())
			{
				if (propertyInfo.PropertyType == typeof(GUISettings))
				{
					GUISettings settings = (GUISettings)propertyInfo.GetValue(skin, null);
					newSkin.settings.cursorColor = settings.cursorColor;
					newSkin.settings.cursorFlashSpeed = settings.cursorFlashSpeed;
					newSkin.settings.doubleClickSelectsWord = settings.doubleClickSelectsWord;
					newSkin.settings.selectionColor = settings.selectionColor;
					newSkin.settings.tripleClickSelectsLine = settings.tripleClickSelectsLine;
				}
				else
				{
					propertyInfo.SetValue(newSkin, propertyInfo.GetValue(skin, null), null);
				}

				if (propertyInfo.PropertyType == typeof(GUIStyle))
				{
					GUIStyle style = (GUIStyle)propertyInfo.GetValue(skin, null);
					GUIStyle newStyle = (GUIStyle)propertyInfo.GetValue(newSkin, null);

					if (style.normal.background != null)
						newStyle.normal.background = CopyTexture2D(style.normal.background);

					if (style.hover.background != null)
						newStyle.hover.background = CopyTexture2D(style.hover.background) ;

					if (style.active.background != null)
						newStyle.active.background = CopyTexture2D(style.active.background);

					if (style.focused.background != null)
						newStyle.focused.background = CopyTexture2D(style.focused.background);

					if (style.onNormal.background != null)
						newStyle.onNormal.background = CopyTexture2D(style.onNormal.background );

					if (style.onHover.background != null)
						newStyle.onHover.background = CopyTexture2D(style.onHover.background);

					if (style.onActive.background != null)
						newStyle.onActive.background = CopyTexture2D(style.onActive.background) ;

					if (style.onFocused.background != null)
						newStyle.onFocused.background = CopyTexture2D(style.onFocused.background);
				}

				if (propertyInfo.PropertyType == typeof(GUIStyle[]))
				{
					GUIStyle[] styles = (GUIStyle[])propertyInfo.GetValue(skin, null);
					GUIStyle[] newStyles = (GUIStyle[])propertyInfo.GetValue(newSkin, null);

					for (int i = 0; i < styles.Length; i++)
					{
						GUIStyle style = styles[i];
						GUIStyle newStyle = newStyles[i];

						if (style.normal.background != null)
							newStyle.normal.background = CopyTexture2D(style.normal.background);

						if (style.hover.background != null)
							newStyle.hover.background = CopyTexture2D(style.hover.background);

						if (style.active.background != null)
							newStyle.active.background = CopyTexture2D(style.active.background);

						if (style.focused.background != null)
							newStyle.focused.background = CopyTexture2D(style.focused.background);

						if (style.onNormal.background != null)
							newStyle.onNormal.background = CopyTexture2D(style.onNormal.background);

						if (style.onHover.background != null)
							newStyle.onHover.background = CopyTexture2D(style.onHover.background);

						if (style.onActive.background != null)
							newStyle.onActive.background = CopyTexture2D(style.onActive.background);

						if (style.onFocused.background != null)
							newStyle.onFocused.background = CopyTexture2D(style.onFocused.background);
					}
				}
			}
			return newSkin;
			Debug.Log("GUISkin copy done!");
		}
#endif
        internal static Texture2D CopyTexture2D(Texture2D originalTexture)
        {
            Texture2D copyTexture = new Texture2D(originalTexture.width, originalTexture.height);
            copyTexture.SetPixels(originalTexture.GetPixels());
            copyTexture.Apply();
            return copyTexture;
        }
    }
}
/**********************************************************************************************************/
/***                                            End of file                                             ***/
/**********************************************************************************************************/
