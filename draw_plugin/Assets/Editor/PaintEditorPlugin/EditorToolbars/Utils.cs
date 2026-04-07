using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Utils : IToolbar
    {
        private Rect rect;
        private float width, height;

        private const string text = "Utils";
        private const string tooltip = "Change color";

        public Color currentColor { get; set; }

        public Utils()
        {
            var app = PaintEditorPlugin.Instance;
            width = 200;
            height = 300;
            rect = new Rect(app.position.width - (width + 20), 100, width, height);
        }

        public void DisplayGUI()
        {
            var app = PaintEditorPlugin.Instance;
            rect = new Rect(app.position.width - (width + 20), 100, width, height);
            GUIContent content = new GUIContent(text, tooltip);
            GUIStyle style = new GUIStyle(GUI.skin.window);

            GUILayout.Window(1, rect, CreateWindow, content, style);
        }

        private void CreateWindow(int id)
        {
            currentColor = EditorGUILayout.ColorField(new GUIContent("Color"), currentColor, true, true, true);
        }
    }
}