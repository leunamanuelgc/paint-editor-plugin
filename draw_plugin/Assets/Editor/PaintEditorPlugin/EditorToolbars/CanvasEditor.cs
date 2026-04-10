using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditorInternal;

namespace UnityEditor.PaintEditor
{
    public class CanvasEditor : IToolbar
    {
        private float _aspectRatio;
        private Rect _rect;

        public float aspectRatio
        {
            get { return _aspectRatio; }
            set
            {
                _aspectRatio = value;
                _rect.height = _rect.width * aspectRatio;
            }
        }

        public Rect rect
        {
            get { return _rect; }
            set { _rect = value; }
        }

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

            //EmptyCanvas();
        }

        private bool IsPosOutOfBounds(Vector2 pos, Vector2 pos0, Vector2 pos1)
        {
            if (pos0.x < pos1.x && pos.x > pos1.x)
            {
                return true;
            }
            else if (pos0.x > pos1.x && pos.x < pos1.x)
            {
                return true;
            }
            else if (pos0.y < pos1.y && pos.y > pos1.y)
            {
                return true;
            }
            else if (pos0.y > pos1.y && pos.y < pos1.y)
            {
                return true;
            }

            return false;
        }

        private void EvaluatePos(float a0, float b0, float a1, float mB, ref float a, out float b)
        {
            if (a0 != a1)
                a += a0 < a1 ? 1 : -1;
            b = mB * (a - a0) + b0;
        }

        private bool IsRectOverTexture(Rect point)
        {
            float sizeX = point.size.x;
            float sizeY = point.size.y;

            if (point.x + sizeX < 0 || point.y + sizeY < 0 || point.x > selectedLayer.texture.width || point.y > selectedLayer.texture.height)
            {
                return false;
            }

            return true;
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
            layerList.Add(new Layer(0, rect));
            selectedLayer = layerList[0];
        }

        public void Resize(float zoomLevel)
        {
            rect = new Rect(rect.position, size * zoomLevel);
        }

        public Vector2 PosInCanvas(float x, float y)
        {
            float new_x = x - rect.x;
            float new_y = rect.height - (y - rect.y);

            return new Vector2(new_x, new_y);
        }

        public Vector2 MousePosInCanvas()
        {
            float x = Event.current.mousePosition.x - rect.x;
            float y = rect.height - (Event.current.mousePosition.y - rect.y);

            return new Vector2(x, y);
        }

        public Vector2 ConvertCanvasPosToTexturePos(Vector2 pos)
        {
            Vector2 convertion = new Vector2(selectedLayer.texture.width / rect.width, selectedLayer.texture.height / rect.height);

            return new Vector2(pos.x * convertion.x, pos.y * convertion.y);
        }

        public void Paint(Color color, Vector2 size)
        {
            Vector2 currentMousePos = MousePosInCanvas();
            Vector2 mouseDelta = Event.current.delta;
            Vector2 prevMousePos = new Vector2(currentMousePos.x - mouseDelta.x, currentMousePos.y + mouseDelta.y);

            Vector2 pos0 = ConvertCanvasPosToTexturePos(prevMousePos);
            Vector2 pos = pos0;
            Vector2 pos1 = ConvertCanvasPosToTexturePos(currentMousePos);

            Vector2 delta = pos1 - pos0;
            Vector2 increment = new Vector2(delta.x / delta.y, delta.y / delta.x);

            while (pos.x != pos1.x || pos.y != pos1.y)
            {
                if (IsPosOutOfBounds(pos, pos0, pos1)) break;

                PaintPixels(color, pos, size);

                // Bresenham's line algorithm to fill gaps between frames
                if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                {
                    EvaluatePos(pos0.x, pos0.y, pos1.x, increment.y, ref pos.x, out pos.y);
                }
                else
                {
                    EvaluatePos(pos0.y, pos0.x, pos1.y, increment.x, ref pos.y, out pos.x);
                }
            }

            PaintPixels(color, pos, size);
            selectedLayer.texture.Apply();
            PaintEditorPlugin.Instance.Repaint();
        }

        public void PaintPixels(Color color, Vector2 pos, Vector2 size)
        {
            Rect pixelsRect = new Rect(pos.x - size.x / 2, pos.y - size.y / 2, size.x, size.y);

            if (IsRectOverTexture(pixelsRect))
            {
                pixelsRect.xMin = Mathf.Max(pixelsRect.xMin, 0);
                pixelsRect.yMin = Mathf.Max(pixelsRect.yMin, 0);
                pixelsRect.xMax = Mathf.Min(pixelsRect.xMax, selectedLayer.texture.width);
                pixelsRect.yMax = Mathf.Min(pixelsRect.yMax, selectedLayer.texture.height);

                Color[] colors;
                colors = new Color[(int)pixelsRect.width * (int)pixelsRect.height];
                for (int j = 0; j < colors.Length; j++)
                {
                    colors[j] = color;
                }
                selectedLayer.texture.SetPixels((int)pixelsRect.x, (int)pixelsRect.y, (int)pixelsRect.width, (int)pixelsRect.height, colors);
            }
        }

        public void DisplayGUI()
        {
            EditorGUI.DrawTextureTransparent(rect, bgTexture, ScaleMode.ScaleAndCrop);

            for (int i = layerList.Count - 1; i >= 0; i--)
            {
                if (layerList[i].isEnabled)
                {
                    GUI.DrawTexture(rect, layerList[i].texture);
                }
            }
        }

        public void Load(Texture2D newTexture)
        {
            size = new Vector2(newTexture.width, newTexture.height);
            aspectRatio = (float)newTexture.width / (float)newTexture.height;

            ResetLayers();
            selectedLayer.texture = new Texture2D(newTexture.width, newTexture.height, newTexture.format, true, false);
            Graphics.CopyTexture(newTexture, selectedLayer.texture);
            selectedLayer.texture.alphaIsTransparency = true;
            selectedLayer.texture.filterMode = FilterMode.Point;

            float newHeight = rect.width / aspectRatio;

            var app = PaintEditorPlugin.Instance;
            rect = new Rect(app.position.width / 2 - app.canvas.rect.width / 2, app.position.height / 2 - newHeight / 2, app.canvas.rect.width, newHeight);
            selectedLayer.rect = rect;
        }

        public byte[] Save()
        {
            Texture2D finalTexture = new Texture2D((int)size.x, (int)size.y, TextureFormat.ARGB32, true);
            Color[] transparent = new Color[(int)size.x * (int)size.y];
            Array.Fill(transparent, new Color(0, 0, 0, 0));
            finalTexture.SetPixels(transparent);

            for (int i = layerList.Count - 1; i >= 0; i--)
            {
                //Not ideal solution but will be changed later -> Probably with a shader
                PaintLayerInTexture(i, finalTexture);
            }

            return finalTexture.EncodeToPNG();
        }

        private void PaintLayerInTexture(int index, Texture2D finalTexture)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int i = 0; i < size.x; i++)
                {
                    Color pixel = layerList[index].texture.GetPixel(i,j);
                    if (pixel.a >= 1)
                    {
                        finalTexture.SetPixel(i, j, pixel);
                    }
                    else if(pixel.a > 0 && pixel.a < 1)
                    {
                        Color texturePixel = finalTexture.GetPixel(i, j);
                        finalTexture.SetPixel(i, j, texturePixel + pixel);  //Adding pixels is not exactly how should work, but I'll let that be for now
                    }
                }
            }
        }

        public void AddLayer(ReorderableList list)
        {
            layerList.Add(new Layer(list.count, rect));
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
