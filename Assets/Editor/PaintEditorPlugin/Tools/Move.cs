using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Move : ITool
    {
        public static string name = "Move";
        public static string iconTextureName = "d_MoveTool";
        public static string tooltip = "Move layers through canvas";
        public static GUIContent guiContent = new GUIContent((Texture)EditorGUIUtility.Load(iconTextureName), tooltip);

        public void Select()
        {
            DisplayOptionsGUI();
        }

        private void DisplayOptionsGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(guiContent);
            EditorGUILayout.EndHorizontal();
        }
    }
}