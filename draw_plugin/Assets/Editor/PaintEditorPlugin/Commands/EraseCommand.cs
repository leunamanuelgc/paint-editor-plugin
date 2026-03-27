using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class EraseCommand : ACommand
    {
        public EraseCommand() { }

        public override bool Execute()
        {
            SaveBackup();

            var app = PaintEditorPlugin.Instance;
            Eraser eraser = (Eraser)app.currentTool;
            app.canvas.Paint(new Color(0,0,0,0), eraser.size);
            return true;
        }
    }
}
