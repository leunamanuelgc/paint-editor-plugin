using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Layer
    {

        public static string iconTextureOn = "d_VisibilityOn";
        public static string iconTextureOff = "d_VisibilityOff";

        public Rect rect {  get; set; }

        public Vector2 realSize { get; set; }

        public RenderTexture rTexture { get; set; }

        public bool isEnabled { get; set; }

        public bool isSelected { get; set; }

        public string name { get; set; }

        public Layer(int num, Rect rect)
        {
            this.rect = new Rect(rect.position, rect.size * PaintEditorPlugin.Instance.GetZoomLevel());
            this.realSize = rect.size;
            this.isEnabled = true;
            this.name = "Layer " + num;

            InitializeTexture((int)rect.width, (int)rect.height);

            PanCommand.OnPanMove += Move;
            Zoom.OnZoomLevelChange += Resize;
        }

        ~Layer()
        {
            Release();
        }

        public void InitializeTexture(int width, int height)
        {
            rTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            rTexture.filterMode = FilterMode.Point;
            rTexture.enableRandomWrite = true;
            rTexture.Create();
        }

        public void Move(Vector2 delta)
        {
            rect = new Rect(rect.x + delta.x, rect.y + delta.y, rect.width, rect.height);
        }

        public void Resize(float zoomLevel)
        {
            rect = new Rect(rect.position, realSize * zoomLevel);
        }

        public void Release()
        {
            rTexture.Release();
            rTexture = null;

            Zoom.OnZoomLevelChange -= Resize;
        }

        public void CopyToLayer(Layer layer)
        {
            layer.name = this.name;
            layer.isSelected = this.isSelected;
            layer.isEnabled = this.isEnabled;
            layer.realSize = this.realSize;
            layer.rect = this.rect;
            Graphics.CopyTexture(this.rTexture, layer.rTexture);
        }
    }
}