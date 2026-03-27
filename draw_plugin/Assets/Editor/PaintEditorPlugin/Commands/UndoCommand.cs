using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class UndoCommand : ACommand
    {
        public UndoCommand() { }

        public override bool Execute()
        {
            SaveBackup();
            PaintEditorPlugin.Instance.Undo();
            return false;
        }
    }
}
