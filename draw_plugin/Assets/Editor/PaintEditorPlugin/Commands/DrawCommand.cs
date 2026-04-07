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
            app.canvas.Paint(app.currentColor, brush.size);
            return true;
        }
    }
}
