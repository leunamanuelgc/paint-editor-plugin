using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Utils : IToolbar
    {
        private Rect rect;

        private const string text = "Utils";
        private const string tooltip = "Change color";

        public Color currentColor { get; set; }

        public Utils()
        {
            var app = PaintEditorPlugin.Instance;
            rect = new Rect(app.position.width - 220, 100, 200, 300);
        }

        public void DisplayGUI()
        {
            var app = PaintEditorPlugin.Instance;
            rect = new Rect(app.position.width - 220, 100, 200, 300);
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