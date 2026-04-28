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
        private EventType eType;

        public FillCommand(Layer layer, Rect canvas, Vector2 canvasSize, Vector2 position, Color color, EventType eType)
        {
            this.layer = layer;
            this.canvas = canvas;
            this.canvasSize = canvasSize;
            this.position = position;
            this.color = color;
            this.eType = eType;
        }

        public override bool Execute()
        {
            if (eType != EventType.MouseDown) return false;

            var pos = ConvertPos(PosInRectInt(position, canvas), canvas, canvasSize);
            var posInt = new Vector2Int((int)pos.x, (int)pos.y);
            var targetColor = layer.GetPixel(posInt.x, posInt.y);
            layer.Fill(posInt, targetColor, color);
            return false;
        }
    }
}
