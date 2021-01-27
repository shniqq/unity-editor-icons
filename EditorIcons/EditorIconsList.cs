using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Packages.EditorIcons
{
    public class EditorIconsList : EditorWindow
    {
        private static readonly List<Texture2D> IconsCache = new List<Texture2D>();
        public static IEnumerable<Texture2D> Icons
        {
            get
            {
                if (!IconsCache.Any())
                {
                    LoadIcons();
                }

                return IconsCache;
            }
        }
        private static string _searchText = string.Empty;
        private Vector2 _scrollPosition = Vector2.zero;

        [MenuItem("Window/Editor Icons")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(EditorIconsList));
            window.titleContent.text = nameof(EditorIconsList);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Icons"))
            {
                LoadIcons();
            }

            if (GUILayout.Button("Generate Icon Enum"))
            {
                var generateEditorIconsEnumWindow = GetWindow<GenerateEditorIconsEnumWindow>();
                generateEditorIconsEnumWindow.titleContent.text = "Editor Icons Enum Creation";
                generateEditorIconsEnumWindow.Show();
            }

            EditorGUILayout.EndHorizontal();

            if (!IconsCache.Any())
            {
                return;
            }

            _searchText = EditorGUILayout.TextField(_searchText);

            EditorGUILayout.Separator();
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            var iconsMatchingSearch = IconsCache
                .Where(icon => string.IsNullOrWhiteSpace(_searchText) || icon.name.Contains(_searchText));
            foreach (var icon in iconsMatchingSearch)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent(icon.name, icon));
                if (GUILayout.Button("Copy"))
                {
                    EditorGUIUtility.systemCopyBuffer = icon.name;
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        private static void LoadIcons()
        {
            EditorUtility.DisplayProgressBar(nameof(EditorIconsList), "Loading..", 0.1f);
            try
            {
                var editorAssetBundle = GetEditorAssetBundle();
                var iconsPath = GetIconsPath();
                IconsCache.Clear();
                IconsCache.AddRange(EnumerateIcons(editorAssetBundle, iconsPath));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static IEnumerable<Texture2D> EnumerateIcons(AssetBundle editorAssetBundle, string iconsPath)
        {
            return editorAssetBundle.GetAllAssetNames()
                .Where(assetName => assetName.StartsWith(iconsPath, StringComparison.OrdinalIgnoreCase))
                .Where(assetName => assetName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                    assetName.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                .Select(editorAssetBundle.LoadAsset<Texture2D>)
                .Where(e => e != null);
        }

        private static AssetBundle GetEditorAssetBundle()
        {
            var editorGUIUtility = typeof(EditorGUIUtility);
            var getEditorAssetBundle = editorGUIUtility.GetMethod(
                "GetEditorAssetBundle",
                BindingFlags.NonPublic | BindingFlags.Static);

            return (AssetBundle) getEditorAssetBundle?.Invoke(null, new object[] { });
        }

        private static string GetIconsPath()
        {
#if UNITY_2018_3_OR_NEWER
            return UnityEditor.Experimental.EditorResources.iconsPath;
#else
            var assembly = typeof(EditorGUIUtility).Assembly;
            var editorResourcesUtility = assembly.GetType("UnityEditorInternal.EditorResourcesUtility");

            var iconsPathProperty = editorResourcesUtility.GetProperty(
                "iconsPath",
                BindingFlags.Static | BindingFlags.Public);

            return (string) iconsPathProperty?.GetValue(null, new object[] { });
#endif
        }
    }
}