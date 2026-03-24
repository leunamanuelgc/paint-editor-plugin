using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class CanvasEditor : AEditorToolbar
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

        public Texture2D texture { get; set; }

        public CanvasEditor(PaintEditorPlugin app, Rect rect, Texture2D texture) : base(app)
        {
            this.rect = rect;
            position = rect.position;
            aspectRatio = rect.width / rect.height;

            this.texture = texture;
            texture.alphaIsTransparency = true;

            Color[] textureColors = new Color[texture.width * texture.height];
            Array.Fill(textureColors, new Color(0, 0, 0, 0));
            texture.SetPixels(textureColors);
            texture.Apply();
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
            Vector2 convertion = new Vector2(texture.width / rect.width, texture.height / rect.height);

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
            texture.Apply();
            app.Repaint();
        }

        public void PaintPixels(Color color, Vector2 pos, Vector2 size)
        {
            Rect pixelsRect = new Rect(pos.x - size.x / 2, pos.y - size.y / 2, size.x, size.y);

            if (IsRectOverTexture(pixelsRect))
            {
                pixelsRect.xMin = Mathf.Max(pixelsRect.xMin, 0);
                pixelsRect.yMin = Mathf.Max(pixelsRect.yMin, 0);
                pixelsRect.xMax = Mathf.Min(pixelsRect.xMax, texture.width);
                pixelsRect.yMax = Mathf.Min(pixelsRect.yMax, texture.height);

                Color[] colors;
                colors = new Color[(int)pixelsRect.width * (int)pixelsRect.height];
                for (int j = 0; j < colors.Length; j++)
                {
                    colors[j] = color;
                }
                texture.SetPixels((int)pixelsRect.x, (int)pixelsRect.y, (int)pixelsRect.width, (int)pixelsRect.height, colors);
            }
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

            if (point.x + sizeX < 0 || point.y + sizeY < 0 || point.x > texture.width || point.y > texture.height)
            {
                return false;
            }

            return true;
        }

        public void DisplayGUI()
        {
            EditorGUI.DrawTextureTransparent(rect, texture, ScaleMode.ScaleToFit, texture.width / texture.height);
        }
    }
}
