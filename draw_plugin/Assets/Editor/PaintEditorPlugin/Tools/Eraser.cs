using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Eraser : Brush
    {
        public Eraser(int minSize, int maxSize, Vector2 size, int typeIndex) : base(minSize, maxSize, size, typeIndex)
        {
            prefixLabelText = "Eraser size";
        }
    }
}
