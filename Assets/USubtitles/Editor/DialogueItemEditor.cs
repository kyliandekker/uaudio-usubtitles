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