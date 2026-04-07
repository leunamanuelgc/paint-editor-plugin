using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Layer
    {
        public static string iconTextureOn = "d_VisibilityOn";
        public static string iconTextureOff = "d_VisibilityOff";

        public Texture2D texture { get; set; }

        public bool isEnabled { get; set; }

        public string name { get; set; }

        public Layer(int num)
        {
            var app = PaintEditorPlugin.Instance;
            texture = new Texture2D((int)app.canvas.size.x, (int)app.canvas.size.y, TextureFormat.ARGB32, true, false);
            isEnabled = true;
            name = "Layer " + num;
        }

    }
}