/*
 *  Copyright(c) 2025 Kylian Dekker
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 *  IN THE SOFTWARE.
 */

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