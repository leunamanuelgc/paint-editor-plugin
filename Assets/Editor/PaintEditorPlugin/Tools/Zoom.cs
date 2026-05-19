using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Zoom : ITool
    {
        private const float minZoom = 0.1f;
        private const float maxZoom = 8f;
        private const string prefixLevelLabel = "Zoom level";
        private float zoom = 1f;
        private float offset = 0f;

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
            EditorGUILayout.LabelField(guiContent, GUILayout.Width(20));

            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(prefixLevelLabel));
            EditorGUIUtility.labelWidth = textDimensions.x + 10;
            zoom = EditorGUILayout.Slider(new GUIContent(prefixLevelLabel), Mathf.Clamp(Mathf.FloorToInt(zoom), 0.5f, maxZoom), minZoom, maxZoom);

            ChangeZoomLevel(zoom * baseZoom);

            EditorGUILayout.EndHorizontal();
        }


        public void SetInitZoom()
        {
            this.offset = 0f;
        }

        public void AddZoom(float zoomToAdd)
        {
            this.offset += zoomToAdd;

            if (Mathf.Abs(this.offset) >= 1f)
            {
                var sign = Mathf.Sign(this.offset);
                zoom = Mathf.Clamp(zoom + sign, 0.5f, maxZoom);

                ChangeZoomLevel(zoom * baseZoom);

                this.offset = 0f;
            }
        }

        public void ChangeZoomLevel(float newZoom)
        {
            if (newZoom != this.zoomLevel)
            {
                this.zoomLevel = newZoom;
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