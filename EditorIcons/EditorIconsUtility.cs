using UnityEditor;
using UnityEngine;

namespace Packages.EditorIcons
{
    public static class EditorIconsUtility
    {
        public const string DarkModePrefix = "d_";

        public static GUIContent GUIContentWithTooltipAndIcon(string text, string tooltip, string icon,
            bool tryLoadDarkIfProSkin = true)
        {
            if (tryLoadDarkIfProSkin && EditorGUIUtility.isProSkin && !icon.StartsWith(DarkModePrefix))
            {
                icon = $"{DarkModePrefix}{icon}";
            }

            return new GUIContent(text, EditorGUIUtility.IconContent(icon).image, tooltip);
        }
    }
}