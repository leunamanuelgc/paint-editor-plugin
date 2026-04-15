using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Eraser : Brush
    {
        new public static string name = "Eraser";
        new public static string iconTextureName = "d_Grid.EraserTool";
        new public static string tooltip = "Erase pixels in canvas";

        public Eraser(int minSize, int maxSize, Vector2Int size, int typeIndex) : base(minSize, maxSize, size, typeIndex)
        {
            prefixLabelText = "Eraser size";
        }
    }
}
