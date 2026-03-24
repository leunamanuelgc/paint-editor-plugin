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

        public int minSize { get; set; }

        public int maxSize { get; set; }

        public Vector2 size { get; set; }

        public int typeIndex { get; set; }

        public Brush(int minSize, int maxSize, Vector2 size, int typeIndex)
        {
            this.minSize = minSize;
            this.maxSize = maxSize;
            this.size = size;
            this.typeIndex = typeIndex;
        }

        public virtual void Select()
        {
            DisplayOptionsGUI();
        }

        private void DisplayOptionsGUI()
        {
            string[] brushTypeOptionsList = { ShapeType.box.ToString(), ShapeType.rect.ToString() };
            typeIndex = EditorGUILayout.Popup(typeIndex, brushTypeOptionsList);
            EditorGUILayout.PrefixLabel("Brush size");

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
