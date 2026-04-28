using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.PaintEditor
{
    public class Layer
    {
        private const int threadSize = 8;

        #region Paint

        // Paint buffer data
        private struct PaintData
        {
            public Vector2Int pos { get; set; }
            public Vector2Int size { get; set; }
            public Color color { get; set; }

            public PaintData(Vector2Int _pos, Vector2Int _size, Color _color)
            {
                pos = _pos; size = _size; color = _color;
            }
        }

        private ComputeShader paintComputeShader;
        private ComputeBuffer paintBuffer;
        private PaintData[] paintData;
        private static readonly int textureId = Shader.PropertyToID("_Texture");
        private static readonly int resolutionId = Shader.PropertyToID("_Resolution");
        private static readonly int paintBufferId = Shader.PropertyToID("_Buffer");
        private static string computePaintPath = PaintEditorPlugin.Instance.ComputePath() + "ComputePaint.compute";
        private static string computePaintFunc = "PlotSize";

        #endregion

        #region Fill

        struct BinData
        {
            public int data;
        }

        // Span filling algorithm pixel data
        private struct SFPixelData
        {
            public int x1 { get; set; }
            public int x2 { get; set; }
            public int y { get; set; }
            public int dy { get; set; }

            public SFPixelData(int _x1, int _x2, int _y, int _dy)
            {
                x1 = _x1;
                x2 = _x2;
                y = _y;
                dy = _dy;
            }
        }

        private ComputeShader fillComputeShader;
        private ComputeBuffer binTextureBuffer;
        private ComputeBuffer fillBuffer;
        private Texture2D onePixelTexture;
        private BinData[] binTextureData;
        private BinData[] textureFillData;
        private static readonly int targetColorId = Shader.PropertyToID("_TargetColor");
        private static readonly int fillColorId = Shader.PropertyToID("_FillColor");
        private static readonly int binaryBufferId = Shader.PropertyToID("_BinTextureBuffer");
        private static readonly int fillBufferId = Shader.PropertyToID("_FillBuffer");
        private static string computeFillPath = PaintEditorPlugin.Instance.ComputePath() + "ComputeFill.compute";
        private static string computeFillFunc = "Fill";
        private static string computeBinTextureFunc = "ComputeBinaryTexture";

        #endregion

        public static string iconTextureOn = "d_VisibilityOn";
        public static string iconTextureOff = "d_VisibilityOff";

        private static int layerExtraSizeMultiplier = 6;

        public Rect rect {  get; set; }

        public RenderTexture rTexture { get; set; }

        public bool isEnabled { get; set; }

        public string name { get; set; }

        public Layer(int num, Rect rect)
        {
            float x = rect.x - rect.width * layerExtraSizeMultiplier / 2 + rect.width / 2;
            float y = rect.y - rect.height * layerExtraSizeMultiplier / 2 + rect.height / 2;

            this.rect = new Rect(x, y, rect.width * layerExtraSizeMultiplier, rect.height * layerExtraSizeMultiplier);
            this.isEnabled = true;
            this.name = "Layer " + num;

            InitializeTextures((int)rect.width * layerExtraSizeMultiplier, (int)rect.height * layerExtraSizeMultiplier);
            InitializeComputeShaders();
        }

        ~Layer()
        {
            Release();
        }

        public void InitializeTextures(int width, int height)
        {
            rTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            rTexture.filterMode = FilterMode.Point;
            rTexture.enableRandomWrite = true;
            rTexture.Create();

            this.onePixelTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            textureFillData = new BinData[rTexture.width * rTexture.height];
            binTextureData = new BinData[rTexture.width * rTexture.height];
        }

        private void InitializeComputeShaders()
        {
            paintComputeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(computePaintPath);
            paintBuffer = new ComputeBuffer(1, Marshal.SizeOf<PaintData>());
            paintData = new PaintData[1];
            paintData[0] = new PaintData(Vector2Int.zero, Vector2Int.one, Color.black);

            fillComputeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(computeFillPath);
            fillBuffer = new ComputeBuffer(rTexture.width * rTexture.height, Marshal.SizeOf<BinData>());
            binTextureBuffer = new ComputeBuffer(rTexture.width * rTexture.height, Marshal.SizeOf<BinData>());
        }

        public Color GetPixel(int x, int y)
        {
            RenderTexture.active = rTexture;
            onePixelTexture.ReadPixels(new Rect(x, y, 1, 1), 0, 0);
            return onePixelTexture.GetPixels()[0];
        }

        public void Fill(Vector2Int pos, Color targetColor, Color fillColor)
        {
            if (GetPixel(pos.x, pos.y) == fillColor) return;

            binTextureData = GetBinaryTexture(targetColor);

            SpanFilling(pos);

            ComputeFill(fillColor);
        }

        private BinData[] GetBinaryTexture(Color targetColor)
        {
            int kernelId = fillComputeShader.FindKernel(computeBinTextureFunc);
            //var canvasSize = PaintEditorPlugin.Instance.canvas.size;
            int groups = Mathf.CeilToInt(rect.size.x / threadSize);

            fillComputeShader.SetVector(resolutionId, new Vector4(rect.size.x, rect.size.y));
            fillComputeShader.SetVector(targetColorId, targetColor);
            fillComputeShader.SetTexture(kernelId, textureId, rTexture);
            fillComputeShader.SetBuffer(kernelId, binaryBufferId, binTextureBuffer);
            fillComputeShader.Dispatch(kernelId, groups, groups, 1);

            BinData[] binData = new BinData[rTexture.width * rTexture.height];
            binTextureBuffer.GetData(binData);

            return binData;
        }

        private void SaveBinaryTexture()
        {
            Texture2D t = new Texture2D(rTexture.width, rTexture.height);
            
            for(int j = 0 ; j < rTexture.height; j++)
            {
                for (int i = 0; i < rTexture.width; i++)
                {
                    if (binTextureData[j * rTexture.width + i].data == 1)
                    {
                        t.SetPixel(i, j, Color.white);
                    }
                    else
                    {
                        t.SetPixel(i, j, Color.black);
                    }
                }
            }

            byte[] data = t.EncodeToPNG();
            var path = Application.dataPath + "/Resources/binaryTexture.png";
            File.WriteAllBytes(path, data);
        }

        private bool Inside(int x, int y)
        {
            if ((x >= 0 && x < rTexture.width) && (y >= 0 && y < rTexture.height))
            {
                if (textureFillData[y * rTexture.width + x].data == 1) return false;

                return binTextureData[y * rTexture.width + x].data == 1;
            }
            return false;
        }

        private void RegisterTexturePixel(int x, int y)
        {
            textureFillData[y * rTexture.width + x].data = 1;
        }

        private void SpanFilling(Vector2Int pos)
        {
            int x = pos.x;
            if (!Inside(pos.x, pos.y)) return;

            Queue<SFPixelData> pixels = new Queue<SFPixelData>();
            pixels.Enqueue(new SFPixelData(x, x, pos.y, 1));
            pixels.Enqueue(new SFPixelData(x, x, pos.y - 1, -1));

            while (pixels.Count > 0)
            {
                SFPixelData px = pixels.Dequeue();
                x = px.x1;
                
                if (Inside(x, px.y))
                {
                    while (Inside(x - 1, px.y))
                    {
                        RegisterTexturePixel(x - 1, px.y);
                        x = x - 1;
                    }

                    if (x < px.x1)
                    {
                        pixels.Enqueue(new SFPixelData(x, px.x1 - 1, px.y - px.dy, -px.dy));
                    }
                }

                while (px.x1 <= px.x2)
                {
                    while (Inside(px.x1, px.y))
                    {
                        RegisterTexturePixel(px.x1, px.y);
                        px.x1 += 1;
                    }

                    if (px.x1 > x)
                    {
                        pixels.Enqueue(new SFPixelData(x, px.x1 - 1, px.y + px.dy, px.dy));
                    }

                    if ((px.x1 - 1) > px.x2)
                    {
                        pixels.Enqueue(new SFPixelData(px.x2 + 1, px.x1 - 1, px.y - px.dy, -px.dy));
                    }

                    px.x1 += 1;

                    while (px.x1 <= px.x2 && !Inside(px.x1, px.y)) px.x1 += 1;

                    x = px.x1;
                }
            }
        }

        private void ComputeFill(Color color)
        {   
            int kernelId = fillComputeShader.FindKernel(computeFillFunc);
            int groups = Mathf.CeilToInt(rect.size.x / threadSize);
            Vector4 resolution = new Vector4(rect.size.x, rect.size.y);

            fillBuffer.SetData(textureFillData);
            fillComputeShader.SetVector(resolutionId, resolution);
            fillComputeShader.SetVector(fillColorId, color);
            fillComputeShader.SetTexture(kernelId, textureId, rTexture);
            fillComputeShader.SetBuffer(kernelId, fillBufferId, fillBuffer);
            fillComputeShader.Dispatch(kernelId, groups, groups, 1);

            for (int i = 0; i < textureFillData.Length; i++)
            {
                textureFillData[i].data = 0;
                binTextureData[i].data = 0;
            }
        }

        public void PaintTexture(Vector2Int pos0, Vector2Int pos1, Vector2Int size, Color color)
        {
            paintData[0].color = color;
            paintData[0].size = size;

            PlotLine(pos0.x, pos0.y, pos1.x, pos1.y);
        }

        private void PlotLine(int x0, int y0, int x1, int y1)
        {
            int dx = Mathf.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;
            int error = dx + dy;

            while (true)
            {
                Plot(new Vector2Int(x0, y0));
                int e2 = 2 * error;
                if (e2 >= dy)
                {
                    if (x0 == x1) break;
                    error += dy;
                    x0 += sx;
                }

                if (e2 <= dx)
                {
                    if (y0 == y1) break;
                    error += dx;
                    y0 += sy;
                }
            }
        }

        private void Plot(Vector2Int pos)
        {
            paintData[0].pos = pos;
            paintBuffer.SetData(paintData);

            int kernelId = paintComputeShader.FindKernel(computePaintFunc);
            int groups = Mathf.CeilToInt(rect.size.x / threadSize);
            Vector4 resolution = new Vector4(rect.size.x, rect.size.y);

            paintComputeShader.SetVector(resolutionId, resolution);
            paintComputeShader.SetTexture(kernelId, textureId, rTexture);
            paintComputeShader.SetBuffer(kernelId, paintBufferId, paintBuffer);
            paintComputeShader.Dispatch(kernelId, groups, groups, 1);
        }

        public void Move(Vector2 delta)
        {
            rect = new Rect(rect.x + delta.x, rect.y + delta.y, rect.width, rect.height);
        }

        public void Release()
        {
            paintBuffer.Release();
            fillBuffer.Release();
            binTextureBuffer.Release();
            rTexture.Release();

            paintBuffer = null;
            fillBuffer = null;
            binTextureBuffer = null;
            paintComputeShader = null;
            textureFillData = null;
            binTextureData = null;
            paintData = null;
            rTexture = null;   
        }
    }
}