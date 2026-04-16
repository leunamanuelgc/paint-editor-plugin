using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class EraseCommand : ACommand
    {
        public EraseCommand() { }

        public override bool Execute()
        {
            var app = PaintEditorPlugin.Instance;
            Eraser eraser = (Eraser)app.toolbox.currentTool;
            var posf = app.ConvertPos(app.PosInRectInt(app.MousePosition(), app.canvas.rect), app.canvas.rect, app.canvas.size);
            var pos1 = new Vector2Int((int)posf.x, (int)posf.y);
            var deltaf = app.ConvertPos(app.DeltaInt(), app.canvas.rect, app.canvas.size);
            var delta = new Vector2Int((int)deltaf.x, (int)deltaf.y);
            var pos0 = new Vector2Int(pos1.x - delta.x, pos1.y + delta.y);
            app.canvas.selectedLayer.PaintTexture(pos0, pos1, eraser.size, new Color(0, 0, 0, 0));
            return true;
        }
    }
}
