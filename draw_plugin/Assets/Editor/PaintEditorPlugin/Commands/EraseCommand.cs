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
            app.canvas.Paint(new Color(0,0,0,0), eraser.size);
            return true;
        }
    }
}
