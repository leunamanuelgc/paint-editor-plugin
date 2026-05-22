using UnityEngine;
using UnityEditor;

namespace PaintEditorExtension
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
            SaveBackup();
            PaintEditorExtension.Instance.history.Push(this);

            layerSelection.Select(canvasRect, canvasRealSize);

            return false;
        }
    }
}
