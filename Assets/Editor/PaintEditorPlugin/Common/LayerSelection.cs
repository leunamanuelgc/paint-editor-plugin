using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.PaintEditor
{
    public class LayerSelection
    {
        public enum SelectionType
        {
            open,
            edit,
            close,
        }

        private const int threadSize = 8;

        public Vector2 initPosition;

        public Rect rect;

        public Rect textureRect;

        public Layer layer;

        public SelectionType selectionType;

        public RenderTexture textureSection;

        #region TakeSection

        private ComputeShader takeSectionComputeShader;
        private static readonly int sourceTextureId = Shader.PropertyToID("_Source");
        private static readonly int destinationTextureId = Shader.PropertyToID("_Destination");
        private static readonly int minLimitsId = Shader.PropertyToID("_MinLimits");
        private static readonly int maxLimitsId = Shader.PropertyToID("_MaxLimits");
        private static readonly int resolutionId = Shader.PropertyToID("_Resolution");
        private static readonly int offsetId = Shader.PropertyToID("_Offset");
        private static string computeTakeSectionPath = PaintEditorPlugin.Instance.ComputePath() + "ComputeLayerSelection.compute";
        private static string computeTakeSectionFunc = "TakeSection";
        private static string computeMergeSectionFunc = "MergeSection";

        #endregion

        public LayerSelection(Vector2 initPosition, CanvasEditor canvas)
        {
            LimitPos(ref initPosition, canvas.rect);

            this.initPosition = initPosition;
            this.rect = new Rect(initPosition, Vector2.zero);
            this.layer = canvas.selectedLayer;
            this.selectionType = SelectionType.open;

            PanCommand.OnPanMove += Move;

            takeSectionComputeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(computeTakeSectionPath);
        }

        private void LimitPos(ref Vector2 position, Rect rect)
        {
            if (position.x < rect.xMin)
            {
                position.x = rect.xMin;
            }
            else if (position.x > rect.xMax)
            {
                position.x = rect.xMax;
            }

            if (position.y < rect.yMin)
            {
                position.y = rect.yMin;
            }
            else if (position.y > rect.yMax)
            {
                position.y = rect.yMax;
            }
        }

        private void LimitRect(ref Rect rect1, Rect rect2)
        {
            if (rect1.xMin < rect2.xMin)
            {
                rect1.xMin = rect2.xMin;
            }

            if (rect1.yMin < rect2.yMin)
            {
                rect1.yMin = rect2.yMin;
            }

            if (rect1.xMax > rect2.xMax)
            {
                rect1.xMax = rect2.xMax;
            }

            if (rect1.yMax > rect2.yMax)
            {
                rect1.yMax = rect2.yMax;
            }
        }

        public void Move(Vector2 delta)
        {
            Rect newRect = new Rect(rect.position + delta, rect.size);

            rect = newRect;
            initPosition += delta;

            if(textureSection != null)
            {
                textureRect = new Rect(textureRect.position + delta, textureRect.size);
            }
        }

        public void Expand(Vector2 position, Rect canvasRect)
        {
            rect.xMin = Mathf.Min(position.x, initPosition.x);
            rect.yMin = Mathf.Min(position.y, initPosition.y);
            rect.xMax = Mathf.Max(position.x, initPosition.x);
            rect.yMax = Mathf.Max(position.y, initPosition.y);

            LimitRect(ref rect, canvasRect);
        }

        public void Select(Rect canvasRect, Vector2 canvasRealSize)
        {
            selectionType = SelectionType.edit;

            var pos0 = CommonPaintEditor.ConvertPos(CommonPaintEditor.PosInRectInt(new Vector2(rect.xMin, rect.yMin), canvasRect), canvasRect, canvasRealSize);
            var pos1 = CommonPaintEditor.ConvertPos(CommonPaintEditor.PosInRectInt(new Vector2(rect.xMax, rect.yMax), canvasRect), canvasRect, canvasRealSize);

            ComputeTakeSection(canvasRealSize, (int)pos0.x, (int)pos1.y, (int)pos1.x, (int)pos0.y);
        }

        private void ComputeTakeSection(Vector2 canvasRealSize, int xMin, int yMin, int xMax, int yMax)
        {
            int kernelId = takeSectionComputeShader.FindKernel(computeTakeSectionFunc);
            var canvasSize = canvasRealSize;
            int groups = Mathf.CeilToInt(canvasSize.x / threadSize);

            textureSection = new RenderTexture((int)canvasSize.x, (int)canvasSize.y, 0, RenderTextureFormat.ARGB32);
            textureSection.filterMode = FilterMode.Point;
            textureSection.enableRandomWrite = true;
            textureSection.Create();

            var minLimit = new Vector2(xMin, yMin);
            var maxLimit = new Vector2(xMax, yMax);

            takeSectionComputeShader.SetTexture(kernelId, sourceTextureId, layer.rTexture);
            takeSectionComputeShader.SetTexture(kernelId, destinationTextureId, textureSection);
            takeSectionComputeShader.SetVector(minLimitsId, minLimit);
            takeSectionComputeShader.SetVector(maxLimitsId, maxLimit);
            takeSectionComputeShader.Dispatch(kernelId, groups, groups, 1);

            this.textureRect = new Rect(PaintEditorPlugin.Instance.canvas.rect);
        }

        struct Data
        {
            public Vector2 idx;
        }

        public void MergeSection(Rect destinationRect)
        {
            int kernelId = takeSectionComputeShader.FindKernel(computeMergeSectionFunc);
            var canvasSize = PaintEditorPlugin.Instance.canvas.realSize;
            int groups = Mathf.CeilToInt(canvasSize.x / threadSize);

            Vector2 offset = textureRect.position - destinationRect.position;
            offset.y = -offset.y;

            takeSectionComputeShader.SetTexture(kernelId, sourceTextureId, textureSection);
            takeSectionComputeShader.SetTexture(kernelId, destinationTextureId, layer.rTexture);
            takeSectionComputeShader.SetVector(resolutionId, canvasSize);
            takeSectionComputeShader.SetVector(offsetId, offset);
            takeSectionComputeShader.Dispatch(kernelId, groups, groups, 1);
        }

        public void DisplayGUI()
        {
            Color color = new Color(0, 1, 1, 0.1f);
            Handles.color = color;

            if (selectionType == SelectionType.close) return;

            Handles.DrawLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin), .1f);
            Handles.DrawLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax), .1f);
            Handles.DrawLine(new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax), .1f);
            Handles.DrawLine(new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMax, rect.yMax), .1f);

            if (selectionType == SelectionType.open)
            {
                EditorGUI.DrawRect(rect, color);
            }
        }

        public void Clear()
        {
            PanCommand.OnPanMove -= Move;
            this.layer = null;
        }

        public bool IsWidthAndHeightGreaterThanZero()
        {
            return rect.width > 0 && rect.height > 0;
        }
    }
}