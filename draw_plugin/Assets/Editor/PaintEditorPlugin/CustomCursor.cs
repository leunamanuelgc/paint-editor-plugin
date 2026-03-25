using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class CustomCursor
    {
        protected PaintEditorPlugin app { get; private set; }

        private Texture2D texture { get; set; }

        public Vector2 size { get; set; }

        public CustomCursor(PaintEditorPlugin app, Vector2Int size)
        {
            this.app = app;
            this.size = size;

            InitTexture(size);

            Brush.onSizeChange += Resize;
            Eraser.onSizeChange += Resize;
        }

        ~CustomCursor()
        {
            Brush.onSizeChange -= Resize;
            Eraser.onSizeChange -= Resize;
        }

        private void InitTexture(Vector2Int size)
        {
            texture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
            texture.alphaIsTransparency = true;

            Color[] colors = new Color[texture.width * texture.height];
            Array.Fill(colors, Color.black);
            texture.SetPixels(colors);

            if (size.x > 1 && size.y > 1)
            {
                Color[] transparent = new Color[(texture.width - 2) * (texture.height - 2)];
                Array.Fill(transparent, new Color(0, 0, 0, 0));
                texture.SetPixels(1, 1, texture.width - 2, texture.height - 2, transparent);
            }

            texture.Apply();
            app.Repaint();
        }

        public void Resize(Vector2Int size)
        {
            this.size = size;
            InitTexture(size);
        }

        public void Render()
        {
            Texture2D transparentCursor = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            transparentCursor.alphaIsTransparency = true;

            Cursor.SetCursor(transparentCursor, Vector2.zero, CursorMode.ForceSoftware);
            EditorGUIUtility.AddCursorRect(app.canvas.rect, MouseCursor.CustomCursor);

            Rect position = new Rect(Event.current.mousePosition - size / 2, size);
            GUI.DrawTexture(position, texture, ScaleMode.ScaleToFit, true);

            app.Repaint();
        }


    }
}