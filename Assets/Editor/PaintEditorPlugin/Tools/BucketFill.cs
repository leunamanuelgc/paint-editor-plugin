
using PlasticGui.WorkspaceWindow.BranchExplorer;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class BucketFill : ITool
    {
        public static string name = "Bucket fill";
        public static string iconTextureName = "d_Grid.FillTool";
        public static string tooltip = "Fill areas with a given color.";
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