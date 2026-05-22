using UnityEngine;

namespace PaintEditorExtension
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
        protected Event e;

        public DrawCommand(RenderTexture rTexture, Rect canvas, Rect limits, Vector2 canvasSize, Vector2 position, Color color, Vector2Int size, Event e)
        {
            this.rTexture = rTexture;
            this.canvas = canvas;
            this.limits = limits;
            this.canvasSize = canvasSize;
            this.position = position;
            this.color = color;
            this.size = size;
            this.e = e;
        }

        public override bool Execute()
        {
            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                if (e.type == EventType.MouseDown)
                {
                    SaveBackup();
                    PaintEditorExtension.Instance.history.Push(this);
                }

                var posf = CommonPaintEditor.ConvertPos(CommonPaintEditor.PosInRectInt(position, canvas), canvas, canvasSize);
                var pos1 = new Vector2Int((int)posf.x, (int)posf.y);
                var deltaf = CommonPaintEditor.ConvertPos(new Vector2(e.delta.x, e.delta.y), canvas, canvasSize);
                var delta = new Vector2Int((int)deltaf.x, (int)deltaf.y);
                var pos0 = new Vector2Int(pos1.x - delta.x, pos1.y + delta.y);

                CommonPaintEditor.PaintTexture(rTexture, canvas, limits, PaintEditorExtension.Instance.canvas.realSize, pos0, pos1, size, color);
                CommonPaintEditor.Release();
            }
            return false;
        }
    }
}
