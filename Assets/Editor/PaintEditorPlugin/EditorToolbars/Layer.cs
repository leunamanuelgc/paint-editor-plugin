using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Layer
    {
        private struct PaintData
        {
            public Vector2Int pos;
            public Vector2Int size;
            public Color color;
        }

        private ComputeShader computeShader;
        private ComputeBuffer buffer;
        private PaintData[] data;

        private static readonly int textureId = Shader.PropertyToID("_Texture");
        private static readonly int resolutionId = Shader.PropertyToID("_Resolution");
        private static readonly int bufferId = Shader.PropertyToID("_Buffer");

        public static string iconTextureOn = "d_VisibilityOn";
        public static string iconTextureOff = "d_VisibilityOff";
        private static string computePath = PaintEditorPlugin.Instance.ComputePath() + "ComputePaint.compute";
        private static string computeFunc = "Plot";

        public Rect rect {  get; set; }

        public RenderTexture rTexture { get; set; }

        public bool isEnabled { get; set; }

        public string name { get; set; }

        public Layer(int num, Rect rect)
        {
            this.rect = rect;
            this.isEnabled = true;
            this.name = "Layer " + num;

            InitializeTexture((int)rect.width, (int)rect.height);
            InitializeComputeShader();
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

        private void InitializeComputeShader()
        {
            computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(computePath);
            buffer = new ComputeBuffer(1, Marshal.SizeOf<PaintData>());
            data = new PaintData[1];
            data[0].pos = Vector2Int.zero;
            data[0].size = Vector2Int.one;
            data[0].color = Color.black;
        }

        public void PaintTexture(Vector2Int pos0, Vector2Int pos1, Vector2Int size, Color color)
        {
            data[0].color = color;
            data[0].size = size;

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
            data[0].pos = pos;
            buffer.SetData(data);

            int kernelId = computeShader.FindKernel(computeFunc);
            var canvasSize = PaintEditorPlugin.Instance.canvas.size;
            int groups = Mathf.CeilToInt(canvasSize.x / 8);
            Vector4 resolution = new Vector4(canvasSize.x, canvasSize.y);

            computeShader.SetVector(resolutionId, resolution);
            computeShader.SetTexture(kernelId, textureId, rTexture);
            computeShader.SetBuffer(kernelId, bufferId, buffer);
            computeShader.Dispatch(kernelId, groups, groups, 1);
        }

        public void Release()
        {
            buffer.Release();
            rTexture.Release();

            buffer = null;
            computeShader = null;
            data = null;
            rTexture = null;   
        }
    }
}