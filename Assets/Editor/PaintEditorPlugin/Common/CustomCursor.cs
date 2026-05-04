using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class CustomCursor
    {
        private Texture2D texture { get; set; }

        public Vector2Int size { get; set; }

        public Vector2Int realSize { get; set; }

        public float zoomLevel { get; set; }

        public CustomCursor(Vector2Int realSize)
        {
            this.realSize = realSize;
            this.zoomLevel = PaintEditorPlugin.Instance.GetZoomLevel();

            InitTexture();

            Brush.onSizeChange += Resize;
            Eraser.onSizeChange += Resize;
            Zoom.OnZoomLevelChange += Resize;
        }

        ~CustomCursor()
        {
            Brush.onSizeChange -= Resize;
            Eraser.onSizeChange -= Resize;
            Zoom.OnZoomLevelChange -= Resize;
        }

        private void InitTexture()
        {
            int width = Mathf.CeilToInt(this.realSize.x * zoomLevel);
            int height = Mathf.CeilToInt(this.realSize.y * zoomLevel);

            width = width <= 0 ? 1 : width;
            height = height <= 0 ? 1 : height;

            this.size = new Vector2Int(width, height);

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
            this.realSize = size;
            InitTexture();
        }

        public void Resize(float zoomLevel)
        {
            this.zoomLevel = zoomLevel;
            InitTexture();
        }

        public void Render()
        {
            if (!PaintEditorPlugin.Instance.IsMouseInCanvas()) return;

            Texture2D transparentCursor = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            transparentCursor.alphaIsTransparency = true;

            Cursor.SetCursor(transparentCursor, Vector2.zero, CursorMode.ForceSoftware);
            EditorGUIUtility.AddCursorRect(PaintEditorPlugin.Instance.canvas.rect, MouseCursor.CustomCursor);

            Rect position = new Rect(Event.current.mousePosition - this.size / 2, this.size);
            GUI.DrawTexture(position, texture, ScaleMode.ScaleToFit, true);
            PaintEditorPlugin.Instance.Repaint();

            UnityEngine.Object.DestroyImmediate(transparentCursor);
        }
    }
}