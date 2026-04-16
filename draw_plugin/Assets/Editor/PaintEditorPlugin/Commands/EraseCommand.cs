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
            var pos1 = app.PosInRectInt(app.MousePosition(), app.canvas.rect);
            var delta = app.DeltaInt();
            var pos0 = new Vector2Int(pos1.x - delta.x, pos1.y + delta.y);
            app.canvas.selectedLayer.PaintTexture(pos0, pos1, eraser.size, new Color(0,0,0,0));
            return true;
        }
    }
}
