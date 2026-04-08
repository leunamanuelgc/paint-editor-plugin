using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Layer
    {
        public static string iconTextureOn = "d_VisibilityOn";
        public static string iconTextureOff = "d_VisibilityOff";

        public Rect rect {  get; set; }

        public Texture2D texture { get; set; }

        public bool isEnabled { get; set; }

        public string name { get; set; }

        public Layer(int num)
        {
            var app = PaintEditorPlugin.Instance;
            this.rect = app.canvas.rect;
            texture = new Texture2D((int)app.canvas.size.x, (int)app.canvas.size.y, TextureFormat.ARGB32, true, false);
            texture.alphaIsTransparency = true;
            texture.filterMode = FilterMode.Point;
            isEnabled = true;
            name = "Layer " + num;
            EmptyTexture();
        }

        public Layer(int num, Rect rect)
        {
            var app = PaintEditorPlugin.Instance;
            this.rect = rect;
            texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, true, false);
            texture.alphaIsTransparency = true;
            texture.filterMode = FilterMode.Point;
            isEnabled = true;
            name = "Layer " + num;
            EmptyTexture();
        }

        private void EmptyTexture()
        {
            Color[] transparent = new Color[texture.width * texture.height];
            Array.Fill(transparent, new Color(0, 0, 0, 0));
            texture.SetPixels(transparent);
            texture.Apply();
        }

    }
}