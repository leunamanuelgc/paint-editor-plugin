using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class EraseCommand : DrawCommand
    {
        public EraseCommand(Layer layer, Rect canvas, Vector2 canvasSize, Vector2 position, Vector2Int size, EventType eType) :
            base(layer, canvas, canvasSize, position, new Color(0,0,0,0), size, eType) { }
    }
}
