using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class EraseCommand : DrawCommand
    {
        public EraseCommand(RenderTexture rTexture, Rect canvas, Rect limits, Vector2 canvasSize, Vector2 position, Vector2Int size, Event e) :
            base(rTexture, canvas, limits, canvasSize, position, new Color(0,0,0,0), size, e) { }
    }
}
