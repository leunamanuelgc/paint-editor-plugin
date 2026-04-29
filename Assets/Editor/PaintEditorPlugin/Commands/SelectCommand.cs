using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class SelectCommand : ACommand
    {
        LayerSelection layerSelection;
        Rect canvasRect;
        Vector2 canvasRealSize;
        public SelectCommand(LayerSelection layerSelection, Rect canvasRect, Vector2 canvasSize)
        {
            this.layerSelection = layerSelection;
            this.canvasRect = canvasRect;
            this.canvasRealSize = canvasSize;
        }

        public override bool Execute()
        {
            layerSelection.Select(canvasRect, canvasRealSize);

            return false;
        }
    }
}
