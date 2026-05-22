using UnityEngine;
using System.Collections.Generic;

namespace PaintEditorExtension
{
    public class AddLayerCommand : ACommand
    {
        int index;
        Rect rect;
        List<Layer> layerList;

        public AddLayerCommand(int index, Rect rect, List<Layer> layerList)
        {
            this.index = index;
            this.rect = rect;
            this.layerList = layerList;
        }

        public override bool Execute()
        {
            SaveBackup();
            PaintEditorExtension.Instance.history.Push(this);

            layerList.Add(new Layer(index, rect));

            return false;
        }
    }
}
