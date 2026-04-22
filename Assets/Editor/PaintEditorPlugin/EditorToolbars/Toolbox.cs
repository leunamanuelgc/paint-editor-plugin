using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Toolbox : IToolbar
    {
        private Rect rect;

        private const string text = "Toolbox";
        private const string tooltip = "Select one of the tools available";

        public ITool currentTool { get; set; }
        public ITool lastTool { get; set; }

        public Brush brush { get; private set; }

        public Eraser eraser { get; private set; }

        public BucketFill bucket { get; private set; }

        public Pan pan { get; private set; }

        public Zoom zoom { get; private set; }

        public Toolbox()
        {
            rect = new Rect(10, 100, 50, 300);

            brush = new Brush(1, 100, Vector2Int.one, 0);
            eraser = new Eraser(1, 100, Vector2Int.one, 0);
            bucket = new BucketFill();
            pan = new Pan(1f);
            zoom = new Zoom(1f);

            currentTool = brush;
            lastTool = currentTool;
        }

        public void DisplayGUI()
        {
            GUIContent content = new GUIContent(text, tooltip);
            GUIStyle style = new GUIStyle(GUI.skin.window);

            GUILayout.Window(0, rect, CreateWindow, content, style);
        }

        public void SelectTool(ITool tool)
        {
            currentTool = tool;
            lastTool = currentTool;
            currentTool.Select();
        }

        private void CreateWindow(int id)
        {
            if (GUILayout.Button(Brush.guiContent))
            {
                SelectTool(brush);
            }

            if (GUILayout.Button(Eraser.guiContent))
            {
                SelectTool(eraser);
            }

            if (GUILayout.Button(BucketFill.guiContent))
            {
                SelectTool(bucket);
            }

            if (GUILayout.Button(Pan.guiContent))
            {
                SelectTool(pan);
            }

            if (GUILayout.Button(Zoom.guiContent))
            {
                SelectTool(zoom);
            }
        }
    }
}