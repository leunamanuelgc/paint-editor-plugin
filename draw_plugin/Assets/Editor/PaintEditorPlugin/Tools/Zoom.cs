using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Zoom : ITool
    {
        private const float minZoom = 0.001f;
        private const float maxZoom = 5f;

        public float zoomLevel;

        public static event Action<float> onZoomLevelChange;

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
            var newZoomLevel = EditorGUILayout.Slider(new GUIContent(prefixLabelText), zoomLevel, minZoom, maxZoom);

            if(newZoomLevel != zoomLevel)
            {
                zoomLevel = newZoomLevel;
                onZoomLevelChange?.Invoke(zoomLevel);
            }
        }
    }
}