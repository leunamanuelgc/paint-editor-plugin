using System.Collections.Generic;

namespace PaintEditorExtension
{
    public class RemoveLayerCommand : ACommand
    {
        int index;
        List<Layer> layerList;

        public RemoveLayerCommand(int index, List<Layer> layerList)
        {
            this.index = index;
            this.layerList = layerList;
        }

        public override bool Execute()
        {
            SaveBackup();
            PaintEditorExtension.Instance.history.Push(this);

            layerList[index].Release();
            layerList.RemoveAt(index);

            return false;
        }
    }
}
