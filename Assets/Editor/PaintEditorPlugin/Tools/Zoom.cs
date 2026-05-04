using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Zoom : ITool
    {
        private const float minZoom = 0.1f;
        private const float maxZoom = 5f;
        private const float speed = 0.01f;
        private const string prefixLevelLabel = "Zoom level";
        private float zoom = 1f;

        public static string name = "Zoom";
        public static string iconTextureName = "d_ViewToolZoom";
        public static string tooltip = "Drag to add zoom to canvas";
        public static GUIContent guiContent = new GUIContent((Texture)EditorGUIUtility.Load(iconTextureName), tooltip);

        public float zoomLevel;

        public float baseZoom;

        public static event Action<float> OnZoomLevelChange;

        public Zoom(float baseZoom)
        {
            this.baseZoom = baseZoom;
            this.zoom = 1f;
            this.zoomLevel = baseZoom * this.zoom;
        }

        public void Select()
        {
            DisplayOptionsGUI();
        }

        private void DisplayOptionsGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(guiContent);
            
            zoom = EditorGUILayout.Slider(new GUIContent(prefixLevelLabel), zoom, minZoom, maxZoom);

            if(zoom * baseZoom != zoomLevel)
            {
                zoomLevel = zoom * baseZoom;
                OnZoomLevelChange?.Invoke(zoomLevel);
            }

            EditorGUILayout.EndHorizontal();
        }

        public void ChangeZoomLevel(float zoomChange)
        {
            zoom = Mathf.Clamp(zoom + zoomChange * speed, minZoom, maxZoom);

            if(zoom * baseZoom != this.zoomLevel)
            {
                this.zoomLevel = zoom * baseZoom;
                OnZoomLevelChange?.Invoke(zoomLevel);
            }
        }

        public void SetBaseZoom(float newBaseZoom)
        {
            this.baseZoom = newBaseZoom;
            this.zoom = 1f;
            this.zoomLevel = this.zoom * this.baseZoom;
            OnZoomLevelChange?.Invoke(zoomLevel);
        }
    }
}