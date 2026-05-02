using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class CommonPaintEditor
    {
        private const int threadSize = 8;

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

        private static Vector2 minLimit;
        private static Vector2 maxLimit;
        private static ComputeShader paintComputeShader;
        private static ComputeBuffer paintBuffer;
        private static PaintData[] paintData;
        private static readonly int textureId = Shader.PropertyToID("_Texture");
        private static readonly int resolutionId = Shader.PropertyToID("_Resolution");
        private static readonly int paintBufferId = Shader.PropertyToID("_Buffer");
        private static readonly int minLimitId = Shader.PropertyToID("_MinLimit");
        private static readonly int maxLimitId = Shader.PropertyToID("_MaxLimit");
        private static string computePaintPath = PaintEditorPlugin.Instance.ComputePath() + "ComputePaint.compute";
        private static string computePaintFunc = "PlotSize";

        #endregion

        public static Vector2Int PosInRectInt(Vector2 pos, Rect rect)
        {
            float new_x = pos.x - rect.x;
            float new_y = rect.height - (pos.y - rect.y);

            return new Vector2Int((int)new_x, (int)new_y);
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

        private static void InitializeComputeShaders()
        {
            paintComputeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(computePaintPath);
            paintBuffer = new ComputeBuffer(1, Marshal.SizeOf<PaintData>());
            paintData = new PaintData[1];
            paintData[0] = new PaintData(Vector2Int.zero, Vector2Int.one, Color.black);
        }

        public static void PaintTexture(RenderTexture rTexture, Rect canvas, Rect limits, Vector2Int pos0, Vector2Int pos1, Vector2Int size, Color color)
        {
            InitializeComputeShaders();

            paintData[0].color = color;
            paintData[0].size = size;

            //Y is inverted because (0,0) is upper left in Unity
            minLimit = new Vector2(limits.xMin, limits.yMax);
            maxLimit = new Vector2(limits.xMax, limits.yMin);

            minLimit = ConvertPos(PosInRectInt(minLimit, canvas), canvas, PaintEditorPlugin.Instance.canvas.realSize);
            maxLimit = ConvertPos(PosInRectInt(maxLimit, canvas), canvas, PaintEditorPlugin.Instance.canvas.realSize);

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
            var canvasSize = PaintEditorPlugin.Instance.canvas.realSize;
            int groups = Mathf.CeilToInt(canvasSize.x / threadSize);
            Vector4 resolution = new Vector4(canvasSize.x, canvasSize.y);

            paintComputeShader.SetVector(resolutionId, resolution);
            paintComputeShader.SetVector(minLimitId, minLimit);
            paintComputeShader.SetVector(maxLimitId, maxLimit);
            paintComputeShader.SetTexture(kernelId, textureId, rTexture);
            paintComputeShader.SetBuffer(kernelId, paintBufferId, paintBuffer);
            paintComputeShader.Dispatch(kernelId, groups, groups, 1);
        }
    }
}