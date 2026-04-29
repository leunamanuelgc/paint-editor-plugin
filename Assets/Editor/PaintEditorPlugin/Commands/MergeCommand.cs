using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class MergeCommand : ACommand
    {
        LayerSelection layerSelection;
        Rect destinationCanvas;

        public MergeCommand(LayerSelection layerSelection, Rect destinationCanvas)
        {
            this.layerSelection = layerSelection;
            this.destinationCanvas = destinationCanvas;
        }

        public override bool Execute()
        {
            layerSelection.MergeSection(destinationCanvas);
            return false;
        }
    }
}
