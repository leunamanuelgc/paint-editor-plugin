using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class FillCommand : ACommand
    {
        public FillCommand() { }

        public override bool Execute()
        {
            var app = PaintEditorPlugin.Instance;
            var pos = app.ConvertPos(app.PosInRectInt(app.MousePosition(), app.canvas.rect), app.canvas.rect, app.canvas.size);
            var posInt = new Vector2Int((int)pos.x, (int)pos.y);
            var targetColor = app.canvas.selectedLayer.GetPixel(posInt.x, posInt.y);
            app.canvas.selectedLayer.Fill(posInt, targetColor, app.utils.currentColor);
            app.Repaint();
            return false;
        }
    }
}
