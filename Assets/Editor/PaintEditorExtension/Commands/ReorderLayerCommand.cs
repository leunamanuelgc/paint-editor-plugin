namespace PaintEditorExtension
{
    public class ReorderLayerCommand : ACommand
    {
        public ReorderLayerCommand() { }

        public override bool Execute()
        {
            SaveBackup();
            PaintEditorExtension.Instance.history.Push(this);

            return false;
        }
    }
}
