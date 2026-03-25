using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Zoom : ITool
    {
        private const float minZoom = 0.001f;
        private const float maxZoom = 5f;

        public float zoomLevel;

        protected string prefixLabelText { get; set; }

        public Zoom(float zoomLevel)
        {
            prefixLabelText = "Zoom level";
            this.zoomLevel = zoomLevel;
        }

        public void Select()
        {
            DisplayOptionsGUI();
        }

        private void DisplayOptionsGUI()
        {
            zoomLevel = EditorGUILayout.Slider(new GUIContent(prefixLabelText), zoomLevel, minZoom, maxZoom);
        }
    }
}