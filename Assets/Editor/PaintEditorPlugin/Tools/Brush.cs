using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Brush : ITool
    {
        protected enum ShapeType
        {
            box,
            rect,
        }

        protected string prefixSizeLabel = "Brush size";

        public static string name = "Brush";
        public static string iconTextureName = "d_Grid.PaintTool";
        public static string tooltip = "Draw pixels in canvas";
        public static GUIContent guiContent = new GUIContent((Texture)EditorGUIUtility.Load(iconTextureName), tooltip);
        public static event Action<Vector2Int> onSizeChange;

        public int minSize { get; set; }

        public int maxSize { get; set; }

        public Vector2Int size { get; set; }

        public int typeIndex { get; set; }

        public Brush(int minSize, int maxSize, Vector2Int size, int typeIndex)
        {
            this.minSize = minSize;
            this.maxSize = maxSize;
            this.size = size;
            this.typeIndex = typeIndex;
        }

        public virtual void Select()
        {
            DisplayOptionsGUI(guiContent, prefixSizeLabel);
        }

        protected void DisplayOptionsGUI(GUIContent gui, string prefixSizeLabel)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(gui, GUILayout.Width(20));

            string[] brushTypeOptionsList = { ShapeType.box.ToString(), ShapeType.rect.ToString() };
            typeIndex = EditorGUILayout.Popup(typeIndex, brushTypeOptionsList);

            EditorGUILayout.PrefixLabel(prefixSizeLabel);
            Vector2Int newSize = Vector2Int.one;
            switch (typeIndex)
            {
                case 0:
                    int brushSize = EditorGUILayout.IntSlider(new GUIContent(""), size.x, minSize, maxSize, GUILayout.Width(500f));
                    newSize = new Vector2Int(brushSize, brushSize);
                    break;
                case 1:
                    int brushSizeX = EditorGUILayout.IntSlider(new GUIContent("X"), size.x, minSize, maxSize, GUILayout.Width(350f));
                    int brushSizeY = EditorGUILayout.IntSlider(new GUIContent("Y"), size.y, minSize, maxSize, GUILayout.Width(350f));
                    newSize = new Vector2Int(brushSizeX, brushSizeY);
                    break;
            }

            if (size.x != newSize.x || size.y != newSize.y)
            {
                size = newSize;
                onSizeChange?.Invoke(newSize);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
