using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class FillCommand : ACommand
    {
        private RenderTexture rTexture;
        private Rect canvasRect;
        private Rect limits;
        private Vector2 canvasSize;
        private Vector2 position;
        private Color color;
        private EventType eType;

        public FillCommand(RenderTexture rTexture, Rect canvasRect, Rect limits, Vector2 canvasSize, Vector2 position, Color color, EventType eType)
        {
            this.rTexture = rTexture;
            this.canvasRect = canvasRect;
            this.limits = limits;
            this.canvasSize = canvasSize;
            this.position = position;
            this.color = color;
            this.eType = eType;
        }

        public override bool Execute()
        {
            if (eType != EventType.MouseDown) return false;

            var pos = CommonPaintEditor.ConvertPos(CommonPaintEditor.PosInRectInt(position, canvasRect), canvasRect, canvasSize);
            var posInt = new Vector2Int((int)pos.x, (int)pos.y);
            var targetColor = CommonPaintEditor.GetPixel(rTexture, posInt.x, posInt.y);
            CommonPaintEditor.Fill(rTexture, canvasRect, limits, PaintEditorPlugin.Instance.canvas.realSize, posInt, targetColor, color);
            CommonPaintEditor.Release();
            return false;
        }
    }
}
