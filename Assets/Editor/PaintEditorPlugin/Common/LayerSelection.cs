using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class LayerSelection
    {
        public enum SelectionType
        {
            open,
            edit,
            transform,
            close,
        }

        public enum HandleType
        {
            upL,
            lowL,
            upR,
            lowR,
            rotate
        }

        private const int scaleHandleSize = 10;
        private const int threadSize = 8;

        public Vector2 initPosition;
        public Vector2 selectionStartPosition;
        public Vector2 selectionStartSize;
        public Vector2 realSize;
        public Rect rect;

        public Layer layer;

        public SelectionType selectionType;

        #region TakeSection

        public CustomRenderTexture textureSection;
        private ComputeShader takeSectionComputeShader;
        private static readonly int sourceTextureId = Shader.PropertyToID("_Source");
        private static readonly int destinationTextureId = Shader.PropertyToID("_Destination");
        private static readonly int minLimitsId = Shader.PropertyToID("_MinLimits");
        private static readonly int maxLimitsId = Shader.PropertyToID("_MaxLimits");
        private static readonly int scaleFactorId = Shader.PropertyToID("_ScaleFactor");
        private static string computeTakeSectionPath = PaintEditorPlugin.Instance.ComputePath() + "ComputeLayerSelection.compute";
        private static string computeTakeSectionFunc = "TakeSection";
        private static string computeMergeSectionFunc = "MergeSection";

        #endregion

        #region RotateTexture

        public CustomRenderTexture rotatedTextureSection;
        public Material rotateMaterial;
        private Shader rotateShader;
        private string shaderName = "PaintEditorPlugin/RotateTexture";
        private float angle = 0;

        #endregion

        public LayerSelection()
        {
            Close();

            PanCommand.OnPanMove += Move;
            takeSectionComputeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(computeTakeSectionPath);
            rotateShader = Shader.Find(shaderName);
            Selection.OnTransformMode += Transform;
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

        private Vector2 GetRealSize()
        {
            return this.rect.size / PaintEditorPlugin.Instance.GetZoomLevel();
        }

        public void Move(Vector2 delta)
        {
            Rect newRect = new Rect(rect.position + delta, rect.size);

            rect = newRect;
            initPosition += delta;

        }

        public void Expand(Vector2 position, Rect canvasRect)
        {
            rect.xMin = Mathf.Min(position.x, initPosition.x);
            rect.yMin = Mathf.Min(position.y, initPosition.y);
            rect.xMax = Mathf.Max(position.x, initPosition.x);
            rect.yMax = Mathf.Max(position.y, initPosition.y);

            LimitRect(ref rect, canvasRect);

            this.realSize = GetRealSize();
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
            int groupsX = Mathf.CeilToInt(canvasSize.x / threadSize);
            int groupsY = Mathf.CeilToInt(canvasSize.y / threadSize);

            int w = Mathf.Max(1,Mathf.RoundToInt(this.realSize.x));
            int h = Mathf.Max(1, Mathf.RoundToInt(this.realSize.y));

            textureSection = new CustomRenderTexture(w, h, RenderTextureFormat.ARGB32);            
            textureSection.filterMode = FilterMode.Point;
            textureSection.enableRandomWrite = true;
            textureSection.Create();

            var minLimit = new Vector2(xMin, yMin);
            var maxLimit = new Vector2(xMax, yMax);

            takeSectionComputeShader.SetTexture(kernelId, sourceTextureId, layer.rTexture);
            takeSectionComputeShader.SetTexture(kernelId, destinationTextureId, textureSection);
            takeSectionComputeShader.SetVector(minLimitsId, minLimit);
            takeSectionComputeShader.SetVector(maxLimitsId, maxLimit);
            takeSectionComputeShader.Dispatch(kernelId, groupsX, groupsY, 1);

            this.rect = new Rect(rect);
            this.selectionStartPosition = this.rect.position;
            this.realSize = GetRealSize();
            this.selectionStartSize = this.realSize;

            rotateMaterial = new Material(rotateShader);
            rotateMaterial.SetTexture("_MainTexture", textureSection);
            Rotate(0);
        }

        public void MergeSection(Rect destinationRect)
        {
            if (rotatedTextureSection != null)
            {
                ApplyRotation();
            }

            int kernelId = takeSectionComputeShader.FindKernel(computeMergeSectionFunc);
            var canvasSize = PaintEditorPlugin.Instance.canvas.realSize;
            int groupsX = Mathf.CeilToInt(canvasSize.x / threadSize);
            int groupsY = Mathf.CeilToInt(canvasSize.y / threadSize);
            
            var minLimit = CommonPaintEditor.ConvertPos(CommonPaintEditor.PosInRect(new Vector2(this.rect.xMin, this.rect.yMin), destinationRect), destinationRect, canvasSize);
            var maxLimit = CommonPaintEditor.ConvertPos(CommonPaintEditor.PosInRect(new Vector2(this.rect.xMax, this.rect.yMax), destinationRect), destinationRect, canvasSize);

            var temp = minLimit.y;
            minLimit.y = maxLimit.y;
            maxLimit.y = temp;
            
            Vector2 scaleFactor = this.realSize / this.selectionStartSize;

            takeSectionComputeShader.SetTexture(kernelId, sourceTextureId, textureSection);
            takeSectionComputeShader.SetTexture(kernelId, destinationTextureId, layer.rTexture);
            takeSectionComputeShader.SetVector(minLimitsId, minLimit);
            takeSectionComputeShader.SetVector(scaleFactorId, scaleFactor);
            takeSectionComputeShader.Dispatch(kernelId, groupsX, groupsY, 1);
        }

        public void DisplayGUI()
        {
            Matrix4x4 matrixBackup = GUI.matrix;
            GUIUtility.RotateAroundPivot(PaintEditorPlugin.Instance.angle, this.rect.center);

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

            if (selectionType == SelectionType.edit)
            {
                Handles.DrawLine(new Vector2(rect.center.x - 20, rect.center.y), new Vector2(rect.center.x + 20, rect.center.y), .1f);
                Handles.DrawLine(new Vector2(rect.center.x, rect.center.y - 20), new Vector2(rect.center.x, rect.center.y + 20), .1f);
            }

            if (selectionType == SelectionType.transform)
            {
                
                Handles.DrawWireCube(new Vector2(rect.xMin, rect.yMin), Vector2.one * (scaleHandleSize + 2));
                Handles.DrawWireCube(new Vector2(rect.xMax, rect.yMin), Vector2.one * (scaleHandleSize + 2));
                Handles.DrawWireCube(new Vector2(rect.xMin, rect.yMax), Vector2.one * (scaleHandleSize + 2));
                Handles.DrawWireCube(new Vector2(rect.xMax, rect.yMax), Vector2.one * (scaleHandleSize + 2));

                Handles.color = new Color(0, 1, 1, .5f);
                Handles.DrawWireCube(new Vector2(rect.xMin, rect.yMin), Vector2.one * scaleHandleSize);
                Handles.DrawWireCube(new Vector2(rect.xMax, rect.yMin), Vector2.one * scaleHandleSize);
                Handles.DrawWireCube(new Vector2(rect.xMin, rect.yMax), Vector2.one * scaleHandleSize);
                Handles.DrawWireCube(new Vector2(rect.xMax, rect.yMax), Vector2.one * scaleHandleSize);

                Handles.color = Color.red;
                Handles.DrawWireDisc(new Vector2(rect.center.x, rect.center.y), Vector3.forward, scaleHandleSize);
            }
            GUI.matrix = matrixBackup;
        }

        public void Open(Vector2 initPosition, CanvasEditor canvas)
        {
            LimitPos(ref initPosition, canvas.rect);

            this.initPosition = initPosition;
            this.realSize = Vector2.zero;
            this.rect = new Rect(initPosition, Vector2.zero);
            this.layer = canvas.selectedLayer;
            this.selectionType = SelectionType.open;
        }

        public void Edit()
        {
            this.selectionType = SelectionType.edit;
        }

        private void Transform()
        {
            this.selectionType = SelectionType.transform;
        }

        public void Close()
        {
            this.initPosition = Vector2.zero;
            this.rect = Rect.zero;
            this.layer = null;
            this.textureSection = null;
            this.rotatedTextureSection = null;
            this.selectionType = SelectionType.close;
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

        public Vector2 GetHandle(HandleType type)
        {
            switch (type)
            {
                case HandleType.upL:
                    return new Vector2(rect.xMin, rect.yMin);
                case HandleType.lowL:
                    return new Vector2(rect.xMin, rect.yMax);
                case HandleType.upR:
                    return new Vector2(rect.xMax, rect.yMin);
                case HandleType.lowR:
                    return new Vector2(rect.xMax, rect.yMax);
                case HandleType.rotate:
                    return new Vector2(rect.center.x, rect.center.y);
                default:
                    return Vector2.one * -1;
            }
        }

        public bool IsPosInHandle(Vector2 pos, Vector2 handlePos, int offset)
        {
            Vector2 handlePosMaxSize = handlePos + Vector2.one * (scaleHandleSize + offset);
            Vector2 handlePosMinSize = handlePos - Vector2.one * (scaleHandleSize + offset);

            return pos.x >= handlePosMinSize.x && pos.x <= handlePosMaxSize.x && pos.y >= handlePosMinSize.y && pos.y <= handlePosMaxSize.y;
        }

        public void Scale(HandleType handleType, Vector2 delta)
        {
            switch (handleType)
            {
                case HandleType.upL:
                    rect.xMin += delta.x;
                    rect.yMin += delta.y;
                    break;
                case HandleType.lowL:
                    rect.xMin += delta.x;
                    rect.yMax += delta.y;
                    break;
                case HandleType.upR:
                    rect.xMax += delta.x;
                    rect.yMin += delta.y;
                    break;
                case HandleType.lowR:
                    rect.xMax += delta.x;
                    rect.yMax += delta.y;
                    break;
            }

            this.realSize = GetRealSize();
        }

        public Vector2 GetCenter()
        {
            return this.rect.center;
        }

        public void Rotate(float angleToAdd)
        {
            angle += angleToAdd;
            if(rotatedTextureSection == null)
            {
                rotatedTextureSection = new CustomRenderTexture((int)realSize.x, (int)realSize.y, RenderTextureFormat.ARGB32);
                rotatedTextureSection.filterMode = FilterMode.Point;
                rotatedTextureSection.Create();
            }

            rotateMaterial.SetFloat("_Rotation", angle);

            Graphics.Blit(textureSection, rotatedTextureSection, rotateMaterial);
        }

        private void ApplyRotation()
        {
            Graphics.Blit(rotatedTextureSection, textureSection);
            angle = 0;
        }
    }
}