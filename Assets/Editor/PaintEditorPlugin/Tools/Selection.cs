using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Selection : ITool
    {
        public static string name = "Selection";
        public static string iconTextureName = "d_Grid.BoxTool";
        public static string tooltip = "Drag to select sections of a layer";
        public static GUIContent guiContent = new GUIContent((Texture)EditorGUIUtility.Load(iconTextureName), tooltip);

        public Selection() { }

        public void Select()
        {
            DisplayOptionsGUI();
        }

        private void DisplayOptionsGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(guiContent, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();
        }
    }
}