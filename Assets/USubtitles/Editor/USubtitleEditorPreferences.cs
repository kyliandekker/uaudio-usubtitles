using UnityEditor;
using UnityEngine;

namespace UAudio.USubtitles.Editor
{
    public class USubtitlePreferences
    {
        public Color Color_LineMarker = new Color32(76, 153, 127, 255);

        public Color Color_NewLineMarker = new Color32(150, 153, 127, 255);

        public Color Color_Waveform = new Color32(144, 209, 255, 255);
    }

    public static class USubtitleEditorVariables
    {
        public const string Version = "1.5";
        public static USubtitlePreferences Preferences = new USubtitlePreferences();
    }

    public class USubtitleEditorPreferences
    {
        private static bool prefsLoaded = false;

        public static void Load()
        {
            USubtitleEditorVariables.Preferences = new USubtitlePreferences();

            SetColor("Color_LineMarker", ref USubtitleEditorVariables.Preferences.Color_LineMarker);

            SetColor("Color_NewLineMarker", ref USubtitleEditorVariables.Preferences.Color_NewLineMarker);

            SetColor("Color_Waveform", ref USubtitleEditorVariables.Preferences.Color_Waveform);

            prefsLoaded = true;
        }

        private static void SetColor(string key, ref Color color)
        {
            if (EditorPrefs.HasKey(key))
            {
                _ = ColorUtility.TryParseHtmlString(EditorPrefs.GetString(key), out color);
            }
        }

        private static void SetFloat(string key, ref float refFloat)
        {
            if (EditorPrefs.HasKey(key))
            {
                refFloat = EditorPrefs.GetFloat(key);
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateSubtitleEditorSettings()
        {
            var provider = new SettingsProvider("Preferences/Subtitle Editor", SettingsScope.User)
            {
                label = "Subtitle Editor",
                guiHandler = (searchContext) =>
                {
                    if (!prefsLoaded)
                    {
                        Load();
                    }

                    EditorGUILayout.LabelField("Subtitle Editor Settings", EditorStyles.boldLabel);

                    EditorGUILayout.LabelField("Version: " + USubtitleEditorVariables.Version);
                    USubtitleEditorVariables.Preferences.Color_LineMarker = EditorGUILayout.ColorField(new GUIContent("Line Marker"), USubtitleEditorVariables.Preferences.Color_LineMarker);
                    USubtitleEditorVariables.Preferences.Color_NewLineMarker = EditorGUILayout.ColorField(new GUIContent("New Line Marker"), USubtitleEditorVariables.Preferences.Color_NewLineMarker);
                    USubtitleEditorVariables.Preferences.Color_Waveform = EditorGUILayout.ColorField(new GUIContent("Waveform"), USubtitleEditorVariables.Preferences.Color_Waveform);

                    if (GUI.changed)
                    {
                        Save();
                    }
                },

                // Optional keywords for search
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "subtitle", "editor", "audio", "text" })
            };

            return provider;
        }

        private static void Save()
        {
            EditorPrefs.SetString("Color_LineMarker", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_LineMarker));

            EditorPrefs.SetString("Color_NewLineMarker", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_NewLineMarker));

            EditorPrefs.SetString("Color_Waveform", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_Waveform));
        }
    }
}