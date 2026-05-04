using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Eraser : Brush
    {
        new protected string prefixSizeLabel = "Eraser size";

        new public static string name = "Eraser";
        new public static string iconTextureName = "d_Grid.EraserTool";
        new public static string tooltip = "Erase pixels in canvas";
        new public static GUIContent guiContent = new GUIContent((Texture)EditorGUIUtility.Load(iconTextureName), tooltip);

        public Eraser(int minSize, int maxSize, Vector2Int size, int typeIndex) : base(minSize, maxSize, size, typeIndex)
        {
            prefixSizeLabel = "Eraser size";
        }

        public override void Select()
        {
            DisplayOptionsGUI();
        }

        protected void DisplayOptionsGUI()
        {
            DisplayOptionsGUI(guiContent, prefixSizeLabel);
        }
    }
}
