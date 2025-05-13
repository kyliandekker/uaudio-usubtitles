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
    [CustomPropertyDrawer(typeof(DialogueItem))]
    public class DialogueItemDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return -1f; // Let Unity handle the height automatically
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var samplePosProp = property.FindPropertyRelative("SamplePosition");
            var textProp = property.FindPropertyRelative("Text");

            var temp = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.PropertyField(samplePosProp);
            GUI.enabled = temp;

            EditorGUILayout.PropertyField(textProp, true);

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(Line))]
    public class LineDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return -1f; // Let Unity handle the height automatically
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginVertical();

                var textProp = property.FindPropertyRelative("Text");
                var newLineProp = property.FindPropertyRelative("NewLine");
                var boldProp = property.FindPropertyRelative("Bold");
                var italicProp = property.FindPropertyRelative("Italic");
                var useColorProp = property.FindPropertyRelative("UseColor");
                var colorProp = property.FindPropertyRelative("Color");

                EditorGUILayout.PropertyField(textProp);
                EditorGUILayout.PropertyField(newLineProp);
                EditorGUILayout.PropertyField(boldProp);
                EditorGUILayout.PropertyField(italicProp);

                EditorGUILayout.PropertyField(useColorProp);

                if (useColorProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(colorProp);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}