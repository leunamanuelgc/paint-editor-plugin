using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditorInternal;

namespace UnityEditor.PaintEditor
{
    public class CanvasEditor : IToolbar
    {
       public float aspectRatio {  get; set; }

        public Rect rect { get; set; }

        public Vector2 position { get; set; }

        public Vector2 size { get; set; }

        public Texture2D bgTexture { get; set; }

        public List<Layer> layerList { get; set; }

        public Layer selectedLayer { get; set; }

        public CanvasEditor(Rect rect)
        {
            var app = PaintEditorPlugin.Instance;

            this.rect = rect;
            position = rect.position;
            aspectRatio = rect.width / rect.height;

            this.size = rect.size;

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
        }

        public void Reinitialize(Vector2 size)
        {
            rect = new Rect(rect.position, size);

            this.size = rect.size;

            ResetLayers();
        }

        public void ResetLayers()
        {
            layerList.Clear();
            layerList.Add(new Layer(0, new Rect(rect.x, rect.y, size.x, size.y)));
            selectedLayer = layerList[0];
        }

        public void Resize(float zoomLevel)
        {
            rect = new Rect(rect.position, size * zoomLevel);
        }

        public void DisplayGUI()
        {
            EditorGUI.DrawTextureTransparent(rect, bgTexture, ScaleMode.ScaleAndCrop);

            for (int i = layerList.Count - 1; i >= 0; i--)
            {
                if (layerList[i].isEnabled)
                {
                    GUI.DrawTexture(rect, layerList[i].rTexture);
                }
            }
        }

        public void Load(Texture2D newTexture)
        {
            size = new Vector2(newTexture.width, newTexture.height);
            aspectRatio = (float)newTexture.width / (float)newTexture.height;

            ResetLayers();
            selectedLayer.InitializeTexture(newTexture.width, newTexture.height);
            Graphics.Blit(newTexture, selectedLayer.rTexture);

            float newHeight = rect.width / aspectRatio;

            var app = PaintEditorPlugin.Instance;
            rect = new Rect(app.position.width / 2 - app.canvas.rect.width / 2, app.position.height / 2 - newHeight / 2, app.canvas.rect.width, newHeight);
            selectedLayer.rect = rect;
        }

        //public byte[] Save()
        //{
        //    Texture2D finalTexture = new Texture2D((int)size.x, (int)size.y, TextureFormat.ARGB32, true);
        //    Color[] transparent = new Color[(int)size.x * (int)size.y];
        //    Array.Fill(transparent, new Color(0, 0, 0, 0));
        //    finalTexture.SetPixels(transparent);

        //    for (int i = layerList.Count - 1; i >= 0; i--)
        //    {
        //        //Not ideal solution but will be changed later -> Probably with a shader
        //        PaintLayerInTexture(i, finalTexture);
        //    }

        //    return finalTexture.EncodeToPNG();
        //}

        private void PaintLayerInTexture(int index, Texture2D finalTexture)
        {
            //for (int j = 0; j < size.y; j++)
            //{
            //    for (int i = 0; i < size.x; i++)
            //    {
            //        Color pixel = layerList[index].texture.GetPixel(i,j);
            //        if (pixel.a >= 1)
            //        {
            //            finalTexture.SetPixel(i, j, pixel);
            //        }
            //        else if(pixel.a > 0 && pixel.a < 1)
            //        {
            //            Color texturePixel = finalTexture.GetPixel(i, j);
            //            finalTexture.SetPixel(i, j, texturePixel + pixel);  //Adding pixels is not exactly how should work, but I'll let that be for now
            //        }
            //    }
            //}
        }

        public void AddLayer(ReorderableList list)
        {
            Rect r = new Rect(rect.x, rect.y, size.x, size.y);
            layerList.Add(new Layer(list.count, r));
        }

        public void RemoveLayer(ReorderableList list)
        {
            var newIndex = (list.selectedIndices[0] - 1) >= 0 ? list.selectedIndices[0] - 1 : 0;
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
