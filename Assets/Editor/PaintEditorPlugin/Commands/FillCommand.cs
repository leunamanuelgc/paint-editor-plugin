using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class FillCommand : ACommand
    {
        private Layer layer;
        private Rect canvas;
        private Vector2 canvasSize;
        private Vector2 position;
        private Color color;

        public FillCommand(Layer layer, Rect canvas, Vector2 canvasSize, Vector2 position, Color color)
        {
            this.layer = layer;
            this.canvas = canvas;
            this.canvasSize = canvasSize;
            this.position = position;
            this.color = color;
        }

        public override bool Execute()
        {
            var pos = ConvertPos(PosInRectInt(position - layer.offset, canvas), canvas, canvasSize);
            var posInt = new Vector2Int((int)pos.x, (int)pos.y);
            var targetColor = layer.GetPixel(posInt.x, posInt.y);
            layer.Fill(posInt, targetColor, color);
            return false;
        }
    }
}
