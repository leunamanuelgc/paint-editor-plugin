using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class DrawCommand : ACommand
    {
        protected RenderTexture rTexture;
        protected Rect canvas;
        protected Rect limits;
        protected Vector2 canvasSize;
        protected Vector2 position;
        protected Color color;
        protected Vector2Int size;
        protected EventType eType;

        public DrawCommand(RenderTexture rTexture, Rect canvas, Rect limits, Vector2 canvasSize, Vector2 position, Color color, Vector2Int size, EventType eType)
        {
            this.rTexture = rTexture;
            this.canvas = canvas;
            this.limits = limits;
            this.canvasSize = canvasSize;
            this.position = position;
            this.color = color;
            this.size = size;
            this.eType = eType;
        }

        public override bool Execute()
        {
            if (eType == EventType.MouseDown || eType == EventType.MouseDrag)
            {
                if (eType == EventType.MouseDown)
                {
                    SaveBackup();
                    PaintEditorPlugin.Instance.history.Push(this);
                }

                var posf = CommonPaintEditor.ConvertPos(CommonPaintEditor.PosInRectInt(position, canvas), canvas, canvasSize);
                var pos1 = new Vector2Int((int)posf.x, (int)posf.y);
                var deltaf = CommonPaintEditor.ConvertPos(CommonPaintEditor.DeltaInt(), canvas, canvasSize);
                var delta = new Vector2Int((int)deltaf.x, (int)deltaf.y);
                var pos0 = new Vector2Int(pos1.x - delta.x, pos1.y + delta.y);

                CommonPaintEditor.PaintTexture(rTexture, canvas, limits, PaintEditorPlugin.Instance.canvas.realSize, pos0, pos1, size, color);
                CommonPaintEditor.Release();
            }
            return false;
        }
    }
}
