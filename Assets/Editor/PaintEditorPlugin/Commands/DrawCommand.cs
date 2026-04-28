using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class DrawCommand : ACommand
    {
        protected Layer layer;
        protected Rect canvas;
        protected Vector2 canvasSize;
        protected Vector2 position;
        protected Color color;
        protected Vector2Int size;

        public DrawCommand(Layer layer, Rect canvas, Vector2 canvasSize, Vector2 position, Color color, Vector2Int size)
        {
            this.layer = layer;
            this.canvas = canvas;
            this.canvasSize = canvasSize;
            this.position = position;
            this.color = color;
            this.size = size;
        }

        public override bool Execute()
        {
            var posf = ConvertPos(PosInRectInt(position, canvas), canvas, canvasSize);
            var pos1 = new Vector2Int((int)posf.x, (int)posf.y);
            var deltaf = ConvertPos(DeltaInt(), canvas, canvasSize);
            var delta = new Vector2Int((int)deltaf.x, (int)deltaf.y);
            var pos0 = new Vector2Int(pos1.x - delta.x, pos1.y + delta.y);
            layer.PaintTexture(pos0, pos1, size, color);
            return true;
        }
    }
}
