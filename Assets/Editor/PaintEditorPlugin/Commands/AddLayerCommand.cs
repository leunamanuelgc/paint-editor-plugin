using UnityEngine;
using System.Collections.Generic;

namespace UnityEditor.PaintEditor
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
            PaintEditorPlugin.Instance.history.Push(this);

            layerList.Add(new Layer(index, rect));

            return false;
        }
    }
}
