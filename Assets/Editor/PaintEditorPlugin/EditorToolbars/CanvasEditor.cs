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
            layerList[0].isSelected = true;

            bgTexture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, true, false);
            bgTexture.alphaIsTransparency = true;
            bgTexture.filterMode = FilterMode.Point;

            Color[] transparent = new Color[bgTexture.width * bgTexture.height];
            Array.Fill(transparent, new Color(0, 0, 0, 0));
            bgTexture.SetPixels(transparent);
            bgTexture.Apply();

            Zoom.OnZoomLevelChange += Resize;
            PanCommand.OnPanMove += Move;
        }

        ~CanvasEditor()
        {
            Zoom.OnZoomLevelChange -= Resize;
            PanCommand.OnPanMove -= Move;
        }
        public void Move(Vector2 delta)
        {
            Rect newRect = new Rect(rect.position + delta, rect.size);

            rect = newRect;
        }

        public void Reinitialize(Vector2 size)
        {
            var app = PaintEditorPlugin.Instance;
            app.history.Clear();
            rect = new Rect(rect.position, size);
            this.realSize = size;

            float appW = app.position.width;
            float appH = app.position.height;
            float w = this.rect.width;
            float h = this.rect.height;
            
            ResetLayers();

            float newBaseZoom;
            if (w < h)
            {
                newBaseZoom = app.GetBaseSizeCanvas() / h;
            }
            else
            {
                newBaseZoom = app.GetBaseSizeCanvas() / w;
            }

            app.ResetEditor(newBaseZoom);

            Vector2 startPos = new Vector2(appW / 2 - w * app.GetBaseZoom() / 2, appH / 2 - h * app.GetBaseZoom() / 2);
            Vector2 diff = startPos - this.rect.position;
            app.ExecuteCommand(new PanCommand(diff, this.rect));
        }

        public void ResetLayers()
        {
            layerList.Clear();
            layerList.Add(new Layer(0, new Rect(rect.x, rect.y, realSize.x, realSize.y)));
            selectedLayer = layerList[0];
            layerList[0].isSelected = true;
        }

        public void Resize(float zoomLevel)
        {
            var app = PaintEditorPlugin.Instance;
            Vector2 newSize = realSize * zoomLevel;
            Vector2 diff = this.rect.size - newSize;

            rect = new Rect(rect.position, newSize);

            app.ExecuteCommand(new PanCommand(diff / 2, rect));

            float appW = app.position.width;
            float appH = app.position.height;
            if (rect.xMax < 100 || rect.xMin > appW - 100 || rect.yMax < 100 || rect.yMin > appH - 100)
            {
                float w = this.rect.width;
                float h = this.rect.height;
                Vector2 startPos = this.rect.position;

                if (rect.xMax < 100)
                {
                    startPos.x = 100 - w * app.GetBaseZoom() / 2;
                }
                else if (rect.xMin > appW - 100)
                {
                    startPos.x = (appW - 100);
                }

                if (rect.yMax < 100)
                {
                    startPos.y = 100 - h * app.GetBaseZoom() / 2;
                }
                else if (rect.yMin > appH - 100)
                {
                    startPos.y = (appH - 100);
                }

                diff = startPos - this.rect.position;
                app.ExecuteCommand(new PanCommand(diff, this.rect));
            }   
        }

        public void DisplayGUI()
        {
            var windowRect = PaintEditorPlugin.Instance.position;

            // Fix for the canvas starting at a point that is not the center
            if (start)
            {
                Vector2 diff = -this.rect.position + (windowRect.size / 2 - this.rect.size / 2);
                PaintEditorPlugin.Instance.ExecuteCommand(new PanCommand(diff, rect));
                start = false;
            }

            EditorGUI.DrawTextureTransparent(rect, bgTexture, ScaleMode.ScaleAndCrop);

            for (int i = layerList.Count - 1; i >= 0; i--)
            {
                if (layerList[i].isEnabled)
                {
                    if (layerList[i].blendMode == Layer.LayerBlendMode.normal)
                    {
                        GUI.DrawTexture(layerList[i].rect, layerList[i].rTexture);
                    }
                    else
                    {
                        EditorGUI.DrawPreviewTexture(layerList[i].rect, layerList[i].rTexture, layerList[i].blendMaterial);
                    }
                    
                    LayerSelection layerSelection = PaintEditorPlugin.Instance.layerSelection;
                    if ((layerSelection.selectionType == LayerSelection.SelectionType.edit ||
                        layerSelection.selectionType == LayerSelection.SelectionType.transform ) &&
                        layerList[i].isSelected)
                    {
                        if(layerSelection.rotatedTextureSection != null)
                        {
                            GUI.DrawTexture(layerSelection.rect, layerSelection.rotatedTextureSection);
                        }
                        else
                        {
                            GUI.DrawTexture(layerSelection.rect, layerSelection.textureSection);
                        }
                    }
                }
            }

            if(CommonPaintEditor.GetPixelSize().x > 16 && CommonPaintEditor.GetPixelSize().y > 16)
            {
                DisplayPixelGuidelines();
            }
        }

        private void DisplayPixelGuidelines()
        {
            var pixelSize = CommonPaintEditor.GetPixelSize();
            Handles.color = new Color(1, 1, 1, .5f);

            for(int i = 0; i <= realSize.x; i++)
            {
                Handles.DrawLine(new Vector2(rect.xMin + i * pixelSize.x, rect.yMin), new Vector2(rect.xMin + i * pixelSize.x, rect.yMax), .001f);
            }

            for (int i = 0; i <= realSize.y; i++)
            {
                Handles.DrawLine(new Vector2(rect.xMin, rect.yMin + i * pixelSize.y), new Vector2(rect.xMax, rect.yMin + i * pixelSize.y), .001f);
            }
        }

        public void Load(Texture2D newTexture)
        {
            var app = PaintEditorPlugin.Instance;
            app.CancelClick(true);
            Reinitialize(new Vector2(newTexture.width, newTexture.height));
            aspectRatio = (float)newTexture.width / (float)newTexture.height;

            Graphics.Blit(newTexture, selectedLayer.rTexture);

            float newHeight = rect.width / aspectRatio;

            rect = new Rect(app.position.width / 2 - app.canvas.rect.width / 2, app.position.height / 2 - newHeight / 2, app.canvas.rect.width, newHeight);
            selectedLayer.rect = rect;
        }

        public void AddLayer(ReorderableList list)
        {
            Rect r = new Rect(rect.position, realSize);
            PaintEditorPlugin.Instance.ExecuteCommand(new AddLayerCommand(list.count, r, layerList));
        }

        public void RemoveLayer(ReorderableList list)
        {
            int newIndex;
            int selectedIndex = list.selectedIndices[0] - 1;

            if (selectedIndex >= 0 && selectedIndex < layerList.Count - 1)
            {
                newIndex = selectedIndex;
            }
            else if(selectedIndex == layerList.Count - 1)
            {
                newIndex = selectedIndex - 1;
            }
            else
            {
                newIndex = 0;
            }

            PaintEditorPlugin.Instance.ExecuteCommand(new RemoveLayerCommand(list.selectedIndices[0], layerList));
            selectedLayer.isSelected = false;
            selectedLayer = layerList[newIndex];
            selectedLayer.isSelected = true;
        }

        public void SelectLayer(ReorderableList list)
        {
            selectedLayer.isSelected = false;
            selectedLayer = layerList[list.index];
            selectedLayer.isSelected = true;
        }

        public bool CanRemove(ReorderableList list)
        {
            return layerList.Count > 1;
        }
    }
}
