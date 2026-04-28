using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class CustomCursor
    {
        private Texture2D texture { get; set; }

        public Vector2Int size { get; set; }

        public float zoomLevel { get; set; }

        public CustomCursor(Vector2Int size)
        {
            this.size = size;
            this.zoomLevel = 1f;

            InitTexture(size);

            Brush.onSizeChange += Resize;
            Eraser.onSizeChange += Resize;
            Zoom.onZoomLevelChange += Resize;
        }

        ~CustomCursor()
        {
            Brush.onSizeChange -= Resize;
            Eraser.onSizeChange -= Resize;
            Zoom.onZoomLevelChange -= Resize;
        }

        private void InitTexture(Vector2Int size)
        {
            int width = size.x * Mathf.CeilToInt(zoomLevel);
            int height = size.y * Mathf.CeilToInt(zoomLevel);

            texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.alphaIsTransparency = true;

            Color[] colors = new Color[width * height];
            Array.Fill(colors, Color.black);
            texture.SetPixels(colors);

            if (width > 1 && height > 1)
            {
                Color[] transparent = new Color[(width - 2) * (height - 2)];
                Array.Fill(transparent, new Color(0, 0, 0, 0));
                texture.SetPixels(1, 1, width - 2, height - 2, transparent);
            }

            texture.Apply();
            PaintEditorPlugin.Instance.Repaint();
        }

        public void Resize(Vector2Int size)
        {
            this.size = size;
            InitTexture(size);
        }

        public void Resize(float zoomLevel)
        {
            this.zoomLevel = zoomLevel;
            InitTexture(size);
        }

        public void Render()
        {
            if (!PaintEditorPlugin.Instance.IsMouseInCanvas()) return;

            Texture2D transparentCursor = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            transparentCursor.alphaIsTransparency = true;

            Cursor.SetCursor(transparentCursor, Vector2.zero, CursorMode.ForceSoftware);
            EditorGUIUtility.AddCursorRect(PaintEditorPlugin.Instance.canvas.rect, MouseCursor.CustomCursor);

            Rect position = new Rect(Event.current.mousePosition - size * Mathf.RoundToInt(zoomLevel) / 2, size * Mathf.CeilToInt(zoomLevel));
            GUI.DrawTexture(position, texture, ScaleMode.ScaleToFit, true);

            UnityEngine.Object.DestroyImmediate(transparentCursor);
        }
    }
}