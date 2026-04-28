using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditorInternal;

namespace UnityEditor.PaintEditor
{
    public class CanvasEditor : IToolbar
    {
        private bool start = true;
        public float aspectRatio {  get; set; }

        public Rect rect { get; set; }

        public Vector2 realSize { get; set; }

        public Texture2D bgTexture { get; set; }

        public List<Layer> layerList { get; set; }

        public Layer selectedLayer { get; set; }

        public CanvasEditor(Rect rect)
        {
            var app = PaintEditorPlugin.Instance;

            this.rect = new Rect(rect);
            aspectRatio = rect.width / rect.height;
            this.realSize = rect.size;

            layerList = new List<Layer>() { new Layer(0, rect) };
            selectedLayer = layerList[0];

            bgTexture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, true, false);
            bgTexture.alphaIsTransparency = true;
            bgTexture.filterMode = FilterMode.Point;

            Color[] transparent = new Color[bgTexture.width * bgTexture.height];
            Array.Fill(transparent, new Color(0, 0, 0, 0));
            bgTexture.SetPixels(transparent);
            bgTexture.Apply();

            Zoom.onZoomLevelChange += Resize;
        }

        public void Move(Vector2 delta)
        {
            rect = new Rect(rect.position + delta, rect.size);
            foreach(var layer in layerList)
            {
                layer.Move(delta);
            }
        }

        public void Reinitialize(Vector2 size)
        {
            rect = new Rect(rect.position, size);

            this.realSize = rect.size;

            ResetLayers();

            PaintEditorPlugin.Instance.SetZoom(1);
        }

        public void ResetLayers()
        {
            layerList.Clear();
            layerList.Add(new Layer(0, new Rect(rect.x, rect.y, realSize.x, realSize.y)));
            selectedLayer = layerList[0];
        }

        public void Resize(float zoomLevel)
        {
            rect = new Rect(rect.position, realSize * zoomLevel);

            foreach(var layer in layerList)
            {
                layer.Resize(layer.realSize * zoomLevel);
            }
        }

        public void DisplayGUI()
        {
            // Fix for the canvas starting at a point that is not the center
            if (start)
            {
                var windowRect = PaintEditorPlugin.Instance.position;
                Move(new Vector2(windowRect.width / 2 - this.rect.width / 2, windowRect.height / 2 - this.rect.height / 2));
                start = false;
            }

            EditorGUI.DrawTextureTransparent(rect, bgTexture, ScaleMode.ScaleAndCrop);

            for (int i = layerList.Count - 1; i >= 0; i--)
            {
                if (layerList[i].isEnabled)
                {
                    GUI.DrawTexture(layerList[i].rect, layerList[i].rTexture);
                }
            }
        }

        public void Load(Texture2D newTexture)
        {
            realSize = new Vector2(newTexture.width, newTexture.height);
            aspectRatio = (float)newTexture.width / (float)newTexture.height;

            ResetLayers();
            selectedLayer.InitializeTextures(newTexture.width, newTexture.height);
            Graphics.Blit(newTexture, selectedLayer.rTexture);

            float newHeight = rect.width / aspectRatio;

            var app = PaintEditorPlugin.Instance;
            rect = new Rect(app.position.width / 2 - app.canvas.rect.width / 2, app.position.height / 2 - newHeight / 2, app.canvas.rect.width, newHeight);
            selectedLayer.rect = rect;
        }

        public void AddLayer(ReorderableList list)
        {
            Rect r = new Rect(rect.position, realSize);
            layerList.Add(new Layer(list.count, r));
        }

        public void RemoveLayer(ReorderableList list)
        {
            var newIndex = (list.selectedIndices[0] - 1) >= 0 ? list.selectedIndices[0] - 1 : 0;
            layerList[list.selectedIndices[0]].Release();
            layerList.RemoveAt(list.selectedIndices[0]);
            selectedLayer = layerList[newIndex];
        }

        public void SelectLayer(ReorderableList list)
        {
            selectedLayer = layerList[list.index];
        }

        public bool CanRemove(ReorderableList list)
        {
            return layerList.Count > 1;
        }
    }
}
