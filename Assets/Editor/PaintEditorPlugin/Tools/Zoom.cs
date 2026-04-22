using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Zoom : ITool
    {
        private const float minZoom = 0.01f;
        private const float maxZoom = 20f;
        private const float speed = 0.01f;
        private const string prefixLevelLabel = "Zoom level";

        public static string name = "Zoom";
        public static string iconTextureName = "d_ViewToolZoom";
        public static string tooltip = "Drag to add zoom to canvas";
        public static GUIContent guiContent = new GUIContent((Texture)EditorGUIUtility.Load(iconTextureName), tooltip);

        public float zoomLevel;

        public static event Action<float> onZoomLevelChange;

        public Zoom(float zoomLevel)
        {
            this.zoomLevel = zoomLevel;
        }

        public void Select()
        {
            DisplayOptionsGUI();
        }

        private void DisplayOptionsGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(guiContent);
            
            var newZoomLevel = EditorGUILayout.Slider(new GUIContent(prefixLevelLabel), zoomLevel, minZoom, maxZoom);

            if(newZoomLevel != zoomLevel)
            {
                zoomLevel = newZoomLevel;
                onZoomLevelChange?.Invoke(zoomLevel);
            }
            EditorGUILayout.EndHorizontal();
        }

        public void ChangeZoomLevel(float zoomChange)
        {
            zoomLevel = Mathf.Clamp(zoomLevel + zoomChange * speed, minZoom, maxZoom);
            onZoomLevelChange?.Invoke(zoomLevel);
        }
    }
}