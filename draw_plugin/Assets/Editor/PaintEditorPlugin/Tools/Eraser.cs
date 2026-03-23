using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Eraser : Brush
    {
        public static Color color { get; } = new Color(0, 0, 0, 0);

        public Eraser(int minSize, int maxSize, Vector2 size, int typeIndex) : base(minSize, maxSize, size, typeIndex) { }

        public override void Select()
        {
            DisplayOptionsGUI();
        }

        private void DisplayOptionsGUI()
        {
            string[] brushTypeOptionsList = { ShapeType.box.ToString(), ShapeType.rect.ToString() };
            typeIndex = EditorGUILayout.Popup(typeIndex, brushTypeOptionsList);
            EditorGUILayout.PrefixLabel("Eraser size");

            switch (typeIndex)
            {
                case 0:
                    int brushSize = EditorGUILayout.IntSlider(new GUIContent(""), (int)size.x, minSize, maxSize);
                    size = new Vector2(brushSize, brushSize);
                    break;
                case 1:
                    int brushSizeX = EditorGUILayout.IntSlider(new GUIContent("X"), (int)size.x, minSize, maxSize);
                    int brushSizeY = EditorGUILayout.IntSlider(new GUIContent("Y"), (int)size.y, minSize, maxSize);
                    size = new Vector2(brushSizeX, brushSizeY);
                    break;
            }
        }
    }
}
