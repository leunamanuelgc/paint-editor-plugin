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
        private static readonly int textureId = Shader.PropertyToID("_Texture");
        private static readonly int resolutionId = Shader.PropertyToID("_Resolution");
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

            InitializeTextures((int)rect.width, (int)rect.height);
            InitializeComputeShaders();

            PanCommand.OnPanMove += Move;
            Zoom.OnZoomLevelChange += Resize;
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
            var canvasSize = PaintEditorPlugin.Instance.canvas.realSize;
            int groups = Mathf.CeilToInt(canvasSize.x / threadSize);

            fillComputeShader.SetVector(resolutionId, new Vector4(canvasSize.x, canvasSize.y));
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
            var canvasSize = PaintEditorPlugin.Instance.canvas.realSize;
            int groups = Mathf.CeilToInt(canvasSize.x / threadSize);
            Vector4 resolution = new Vector4(canvasSize.x, canvasSize.y);

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
            fillBuffer.Release();
            binTextureBuffer.Release();
            rTexture.Release();

            fillBuffer = null;
            binTextureBuffer = null;
            textureFillData = null;
            binTextureData = null;
            rTexture = null;

            Zoom.OnZoomLevelChange -= Resize;
        }
    }
}