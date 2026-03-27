using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class DrawCommand : ACommand
    {
        public DrawCommand() { }

        public override bool Execute()
        {
            SaveBackup();

            var app = PaintEditorPlugin.Instance;
            Brush brush = (Brush)app.currentTool;
            app.canvas.Paint(app.currentColor, brush.size);
            return true;
        }
    }
}
