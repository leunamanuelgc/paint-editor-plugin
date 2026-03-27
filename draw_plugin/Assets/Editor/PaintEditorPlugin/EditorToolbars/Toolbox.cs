using UnityEngine;
using static UnityEngine.GridBrushBase;

namespace UnityEditor.PaintEditor
{
    public class Toolbox : IToolbar
    {
        private Rect rect;

        public ITool currentTool { get; set; }
        public ITool lastTool { get; set; }

        public Brush brush { get; private set; }

        public Eraser eraser { get; private set; }

        public Pan pan { get; private set; }

        public Zoom zoom { get; private set; }

        public Toolbox()
        {
            //rect = 

            brush = new Brush(1, 100, Vector2.one, 0);
            eraser = new Eraser(1, 100, Vector2.one, 0);
            pan = new Pan(1f);
            zoom = new Zoom(1f);

            currentTool = brush;
            lastTool = currentTool;
        }

        public void DisplayGUI()
        {
            EditorGUILayout.BeginVertical();

            //Create Window with rect size
            //GUILayout.Window(1)

            if (EditorGUILayout.DropdownButton(new GUIContent("Brush"), FocusType.Keyboard, EditorStyles.toolbarButton))
            {
                SelectTool(brush);
            }

            if (EditorGUILayout.DropdownButton(new GUIContent("Eraser"), FocusType.Keyboard, EditorStyles.toolbarButton))
            {
                SelectTool(eraser);
            }

            if (EditorGUILayout.DropdownButton(new GUIContent("Pan"), FocusType.Keyboard, EditorStyles.toolbarButton))
            {
                SelectTool(pan);
            }

            if (EditorGUILayout.DropdownButton(new GUIContent("Zoom"), FocusType.Keyboard, EditorStyles.toolbarButton))
            {
                SelectTool(zoom);
            }

            EditorGUILayout.EndVertical();
        }

        public void SelectTool(ITool tool)
        {
            currentTool = tool;
            lastTool = currentTool;
            currentTool.Select();
        }
    }
}