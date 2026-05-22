using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;

namespace PaintEditorExtension
{
    public class CommonPaintEditor
    {
        #region Compute

        private const int threadSize = 8;

        private static Vector2 realSize;
        private static Vector2 minLimit;
        private static Vector2 maxLimit;
        private static readonly int textureId = Shader.PropertyToID("_Texture");
        private static readonly int resolutionId = Shader.PropertyToID("_Resolution");
        private static readonly int minLimitId = Shader.PropertyToID("_MinLimit");
        private static readonly int maxLimitId = Shader.PropertyToID("_MaxLimit");

        public static void SetLimits(Rect limits, Rect canvasRect, Vector2 realSize)
        {
            //Y is inverted because (0,0) is upper left in Unity
            minLimit = new Vector2(limits.xMin, limits.yMax);
            maxLimit = new Vector2(limits.xMax, limits.yMin);

            minLimit = ConvertPos(PosInRectInt(minLimit, canvasRect), canvasRect, realSize);
            maxLimit = ConvertPos(PosInRectInt(maxLimit, canvasRect), canvasRect, realSize);
        }

        #region Paint

        // Paint buffer data
        private struct PaintData
        {
            public Vector2Int pos;
            public Vector2Int size;
            public Color color;

            public PaintData(Vector2Int _pos, Vector2Int _size, Color _color)
            {
                pos = _pos; size = _size; color = _color;
            }
        }

        private static ComputeShader paintComputeShader;
        private static ComputeBuffer paintBuffer;
        private static PaintData[] paintData;
        private static readonly int paintBufferId = Shader.PropertyToID("_Buffer");
        private static string computePaintPath = PaintEditorExtension.Instance.ComputePath() + "ComputePaint.compute";
        private static string computePaintFunc = "PlotSize";

        private static void InitializePaintData(Vector2 rSize)
        {
            realSize = rSize;
            paintComputeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(computePaintPath);
            paintBuffer = new ComputeBuffer(1, Marshal.SizeOf<PaintData>());
            paintData = new PaintData[1];
            paintData[0] = new PaintData(Vector2Int.zero, Vector2Int.one, Color.black);
        }

        public static void PaintTexture(RenderTexture rTexture, Rect canvasRect, Rect limits, Vector2 realSize, Vector2Int pos0, Vector2Int pos1, Vector2Int size, Color color)
        {
            InitializePaintData(realSize);
            SetLimits(limits, canvasRect, realSize);

            paintData[0].color = color;
            paintData[0].size = size;

            PlotLine(rTexture, pos0.x, pos0.y, pos1.x, pos1.y);
        }

        private static void PlotLine(RenderTexture rTexture, int x0, int y0, int x1, int y1)
        {
            int dx = Mathf.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;
            int error = dx + dy;

            while (true)
            {
                Plot(rTexture, new Vector2Int(x0, y0));
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

        private static void Plot(RenderTexture rTexture, Vector2Int pos)
        {
            paintData[0].pos = pos;

            paintBuffer.SetData(paintData);

            int kernelId = paintComputeShader.FindKernel(computePaintFunc);
            int groupsX = Mathf.CeilToInt((float)rTexture.width / (float)threadSize);
            int groupsY = Mathf.CeilToInt((float)rTexture.height / (float)threadSize);
            if (groupsX <= 0) groupsX = 1;
            if (groupsY <= 0) groupsY = 1;
            Vector4 resolution = new Vector4(realSize.x, realSize.y);

            paintComputeShader.SetVector(resolutionId, resolution);
            paintComputeShader.SetVector(minLimitId, minLimit);
            paintComputeShader.SetVector(maxLimitId, maxLimit);
            paintComputeShader.SetTexture(kernelId, textureId, rTexture);
            paintComputeShader.SetBuffer(kernelId, paintBufferId, paintBuffer);
            paintComputeShader.Dispatch(kernelId, groupsX, groupsY, 1);
        }

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

        private static ComputeShader fillComputeShader;
        private static ComputeBuffer binTextureBuffer;
        private static ComputeBuffer fillBuffer;
        private static BinData[] binTextureData;
        private static BinData[] textureFillData;

        private static readonly int targetColorId = Shader.PropertyToID("_TargetColor");
        private static readonly int fillColorId = Shader.PropertyToID("_FillColor");
        private static readonly int binaryBufferId = Shader.PropertyToID("_BinTextureBuffer");
        private static readonly int fillBufferId = Shader.PropertyToID("_FillBuffer");
        private static string computeFillPath = PaintEditorExtension.Instance.ComputePath() + "ComputeFill.compute";
        private static string computeFillFunc = "Fill";
        private static string computeBinTextureFunc = "ComputeBinaryTexture";

        public static void InitializeFillData(int width, int height, Vector2 rSize)
        {
            realSize = rSize;
            textureFillData = new BinData[width * height];
            binTextureData = new BinData[width * height];
            fillComputeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(computeFillPath);
            fillBuffer = new ComputeBuffer(width * height, Marshal.SizeOf<BinData>());
            binTextureBuffer = new ComputeBuffer(width * height, Marshal.SizeOf<BinData>());
        }

        public static Color GetPixel(RenderTexture rTexture, int x, int y)
        {
            RenderTexture.active = rTexture;
            Texture2D onePixelTexture;
            onePixelTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            onePixelTexture.ReadPixels(new Rect(x, y, 1, 1), 0, 0);
            return onePixelTexture.GetPixels()[0];
        }

        public static void Fill(RenderTexture rTexture, Rect canvasRect, Rect limits, Vector2 realSize, Vector2Int pos, Color targetColor, Color fillColor)
        {
            InitializeFillData(rTexture.width, rTexture.height, realSize);
            SetLimits(limits, canvasRect, realSize);
            if (GetPixel(rTexture, pos.x, pos.y) == fillColor) return;

            binTextureData = GetBinaryTexture(rTexture, targetColor);
            SpanFilling(rTexture, pos);
            ComputeFill(rTexture,fillColor);
        }

        private static BinData[] GetBinaryTexture(RenderTexture rTexture, Color targetColor)
        {
            int kernelId = fillComputeShader.FindKernel(computeBinTextureFunc);
            int groupsX = Mathf.CeilToInt((float)rTexture.width / (float)threadSize);
            int groupsY = Mathf.CeilToInt((float)rTexture.height / (float)threadSize);
            if (groupsX <= 0) groupsX = 1;
            if (groupsY <= 0) groupsY = 1;

            Vector2 resolution = new Vector2(Mathf.CeilToInt(rTexture.width), Mathf.CeilToInt(rTexture.height));

            fillComputeShader.SetVector(resolutionId, resolution);
            fillComputeShader.SetVector(targetColorId, targetColor);
            fillComputeShader.SetVector(minLimitId, minLimit);
            fillComputeShader.SetVector(maxLimitId, maxLimit);
            fillComputeShader.SetTexture(kernelId, textureId, rTexture);
            fillComputeShader.SetBuffer(kernelId, binaryBufferId, binTextureBuffer);
            fillComputeShader.Dispatch(kernelId, groupsX, groupsY, 1);

            BinData[] binData = new BinData[rTexture.width * rTexture.height];
            binTextureBuffer.GetData(binData);

            return binData;
        }

        private static void SaveBinaryTexture(RenderTexture rTexture)
        {
            Texture2D t = new Texture2D(rTexture.width, rTexture.height);

            for (int j = 0; j < rTexture.height; j++)
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

        private static bool Inside(RenderTexture rTexture, int x, int y)
        {
            if ((x >= 0 && x < rTexture.width) && (y >= 0 && y < rTexture.height))
            {
                if (textureFillData[y * rTexture.width + x].data == 1) return false;

                return binTextureData[y * rTexture.width + x].data == 1;
            }
            return false;
        }

        private static void RegisterTexturePixel(RenderTexture rTexture, int x, int y)
        {
            textureFillData[y * rTexture.width + x].data = 1;
        }

        private static void SpanFilling(RenderTexture rTexture, Vector2Int pos)
        {
            int x = pos.x;
            if (!Inside(rTexture, pos.x, pos.y)) return;

            Queue<SFPixelData> pixels = new Queue<SFPixelData>();
            pixels.Enqueue(new SFPixelData(x, x, pos.y, 1));
            pixels.Enqueue(new SFPixelData(x, x, pos.y - 1, -1));

            while (pixels.Count > 0)
            {
                SFPixelData px = pixels.Dequeue();
                x = px.x1;

                if (Inside(rTexture, x, px.y))
                {
                    while (Inside(rTexture, x - 1, px.y))
                    {
                        RegisterTexturePixel(rTexture, x - 1, px.y);
                        x = x - 1;
                    }

                    if (x < px.x1)
                    {
                        pixels.Enqueue(new SFPixelData(x, px.x1 - 1, px.y - px.dy, -px.dy));
                    }
                }

                while (px.x1 <= px.x2)
                {
                    while (Inside(rTexture, px.x1, px.y))
                    {
                        RegisterTexturePixel(rTexture, px.x1, px.y);
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

                    while (px.x1 <= px.x2 && !Inside(rTexture, px.x1, px.y)) px.x1 += 1;

                    x = px.x1;
                }
            }
        }

        private static void ComputeFill(RenderTexture rTexture, Color color)
        {
            int kernelId = fillComputeShader.FindKernel(computeFillFunc);
            int groupsX = Mathf.CeilToInt((float)rTexture.width / (float)threadSize);
            int groupsY = Mathf.CeilToInt((float)rTexture.height / (float)threadSize);
            if (groupsX <= 0) groupsX = 1;
            if (groupsY <= 0) groupsY = 1;
            Vector4 resolution = new Vector4(Mathf.CeilToInt(rTexture.width), Mathf.CeilToInt(rTexture.height));

            fillBuffer.SetData(textureFillData);
            fillComputeShader.SetVector(resolutionId, resolution);
            fillComputeShader.SetVector(fillColorId, color);
            fillComputeShader.SetTexture(kernelId, textureId, rTexture);
            fillComputeShader.SetBuffer(kernelId, fillBufferId, fillBuffer);
            fillComputeShader.Dispatch(kernelId, groupsX, groupsY, 1);

            for (int i = 0; i < textureFillData.Length; i++)
            {
                textureFillData[i].data = 0;
                binTextureData[i].data = 0;
            }
        }

        #endregion

        #endregion

        public static Vector2Int PosInRectInt(Vector2 pos, Rect rect)
        {
            float new_x = pos.x - rect.x;
            float new_y = rect.height - (pos.y - rect.y);

            return new Vector2Int((int)new_x, (int)new_y);
        }

        public static Vector2 PosInRect(Vector2 pos, Rect rect)
        {
            float new_x = pos.x - rect.x;
            float new_y = rect.height - (pos.y - rect.y);

            return new Vector2(new_x, new_y);
        }

        public static Vector2Int DeltaInt()
        {
            Vector2Int delta = new Vector2Int((int)Event.current.delta.x, (int)Event.current.delta.y);
            return delta;
        }

        public static Vector2 ConvertPos(Vector2 pos, Rect r, Vector2 size)
        {
            Vector2 convertion = new Vector2(size.x / r.width, size.y / r.height);

            return new Vector2(pos.x * convertion.x, pos.y * convertion.y);
        }

        public static void Release()
        {
            if (paintBuffer != null) paintBuffer.Release();
            if (fillBuffer != null) fillBuffer.Release();
            if (binTextureBuffer != null) binTextureBuffer.Release();

            paintBuffer = null;
            fillBuffer = null;
            binTextureBuffer = null;
            textureFillData = null;
            binTextureData = null;
        }

        public static Vector2 GetPixelSize()
        {
            var app = PaintEditorExtension.Instance;

            return app.canvas.rect.size / app.canvas.realSize;
        }
    }
}