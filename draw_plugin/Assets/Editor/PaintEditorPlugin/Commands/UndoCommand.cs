using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class UndoCommand : ACommand
    {
        public UndoCommand(PaintEditorPlugin app) : base(app) { }

        public override bool Execute()
        {
            SaveBackup();
            app.Undo();
            return false;
        }
    }
}
