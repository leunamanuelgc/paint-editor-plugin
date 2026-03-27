using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Zoom : ITool
    {
        private const float minZoom = 0.01f;
        private const float maxZoom = 20f;
        private const float speed = 0.01f;

        public static string name = "Zoom";
        public static string iconTextureName = "d_ViewToolZoom";
        public static string tooltip = "Drag to add zoom to canvas";

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

        public void ChangeZoomLevel(float zoomChange)
        {
            EditorGUILayout.BeginHorizontal();
            zoomLevel = Mathf.Clamp(zoomLevel + zoomChange * speed, minZoom, maxZoom);
            onZoomLevelChange?.Invoke(zoomLevel);
            EditorGUILayout.EndHorizontal();
        }
    }
}