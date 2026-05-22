using System;
using UnityEngine;
using UnityEditor;

namespace PaintEditorExtension
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

            string[] brushTypeOptionsList = Enum.GetNames(typeof(ShapeType));
            typeIndex = EditorGUILayout.Popup(typeIndex, brushTypeOptionsList, GUILayout.MaxWidth(100));

            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(prefixSizeLabel));
            EditorGUIUtility.labelWidth = textDimensions.x + 10;
            EditorGUILayout.PrefixLabel(prefixSizeLabel);
            Vector2Int newSize = Vector2Int.one;
            switch (typeIndex)
            {
                case 0:
                    int brushSize = EditorGUILayout.IntSlider(size.x, minSize, maxSize);
                    newSize = new Vector2Int(brushSize, brushSize);
                    break;
                case 1:
                    textDimensions = GUI.skin.label.CalcSize(new GUIContent("X"));
                    EditorGUIUtility.labelWidth = textDimensions.x + 10;
                    int brushSizeX = EditorGUILayout.IntSlider("X", size.x, minSize, maxSize);
                    int brushSizeY = EditorGUILayout.IntSlider("Y", size.y, minSize, maxSize);
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
