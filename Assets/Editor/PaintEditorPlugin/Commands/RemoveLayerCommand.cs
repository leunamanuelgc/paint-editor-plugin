using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;

namespace UnityEditor.PaintEditor
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
            PaintEditorPlugin.Instance.history.Push(this);

            layerList[index].Release();
            layerList.RemoveAt(index);

            return false;
        }
    }
}
