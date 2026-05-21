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
            texture = new Texture2D(1, 1);

            ReinitTexture();

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

        private void ReinitTexture()
        {
            int width = Mathf.CeilToInt(this.realSize.x * zoomLevel);
            int height = Mathf.CeilToInt(this.realSize.y * zoomLevel);

            width = width <= 0 ? 2 : width;
            height = height <= 0 ? 2 : height;

            this.size = new Vector2Int(width, height);
            
            texture.Reinitialize(width, height, TextureFormat.ARGB32, false);
            texture.alphaIsTransparency = true;

            Color[] colors = new Color[width * height];
            Array.Fill(colors, Color.gray);
            texture.SetPixels(colors);

            if (width > 2 && height > 2)
            {
                Color[] transparent = new Color[(width - 2) * (height - 2)];
                Array.Fill(transparent, Color.black);
                texture.SetPixels(1, 1, width - 2, height - 2, transparent);
            }
            
            if (width > 3 && height > 3)
            {
                Color[] transparent = new Color[(width - 4) * (height - 4)];
                Array.Fill(transparent, new Color(0, 0, 0, 0));
                texture.SetPixels(2, 2, width - 4, height - 4, transparent);
            }

            texture.Apply();
            PaintEditorPlugin.Instance.Repaint();
        }

        public void Resize(Vector2Int size)
        {
            this.realSize = size;
            ReinitTexture();
        }

        public void Resize(float zoomLevel)
        {
            this.zoomLevel = zoomLevel;
            ReinitTexture();
        }

        public void Render()
        {
            PaintEditorPlugin.Instance.Repaint();
            if (!PaintEditorPlugin.Instance.IsMouseInCanvas()) return;

            Texture2D transparentCursor = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            transparentCursor.alphaIsTransparency = true;

            Cursor.SetCursor(transparentCursor, Vector2.zero, CursorMode.ForceSoftware);
            EditorGUIUtility.AddCursorRect(PaintEditorPlugin.Instance.canvas.rect, MouseCursor.CustomCursor);

            Rect position = new Rect(Event.current.mousePosition - this.size / 2, this.size);
            GUI.DrawTexture(position, texture, ScaleMode.ScaleToFit, true);
            

            UnityEngine.Object.DestroyImmediate(transparentCursor);
        }
    }
}