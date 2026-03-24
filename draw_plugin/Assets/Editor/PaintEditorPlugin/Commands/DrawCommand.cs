using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class DrawCommand : ACommand
    {
        public DrawCommand(PaintEditorPlugin app) : base(app) { }

        public override bool Execute()
        {
            SaveBackup();
            Brush brush = (Brush)app.currentTool;
            app.canvas.Paint(app.currentColor, brush.size);
            return true;
        }
    }
}
