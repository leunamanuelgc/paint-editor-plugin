using UnityEngine;
namespace UnityEditor.PaintEditor
{
    public class ReorderLayerCommand : ACommand
    {
        public ReorderLayerCommand() { }

        public override bool Execute()
        {
            SaveBackup();
            PaintEditorPlugin.Instance.history.Push(this);

            return false;
        }
    }
}
