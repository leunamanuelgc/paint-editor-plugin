using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class EraseCommand : ACommand
    {
        public EraseCommand(PaintEditorPlugin app) : base(app) { }

        public override bool Execute()
        {
            SaveBackup();
            Eraser eraser = (Eraser)app.currentTool;
            app.canvas.Paint(new Color(0,0,0,0), eraser.size);
            return true;
        }
    }
}
