using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class DrawCommand : ACommand
    {
        public DrawCommand() { }

        public override bool Execute()
        {
            var app = PaintEditorPlugin.Instance;
            Brush brush = (Brush)app.toolbox.currentTool;
            var posf = app.ConvertPos(app.PosInRectInt(app.MousePosition(), app.canvas.rect), app.canvas.rect, app.canvas.size);
            var pos1 = new Vector2Int((int)posf.x, (int)posf.y);
            var deltaf = app.ConvertPos(app.DeltaInt(), app.canvas.rect, app.canvas.size);
            var delta = new Vector2Int((int)deltaf.x, (int)deltaf.y);
            var pos0 = new Vector2Int(pos1.x - delta.x, pos1.y + delta.y);
            app.canvas.selectedLayer.PaintTexture(pos0, pos1, brush.size, app.utils.currentColor);
            return true;
        }
    }
}
