using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public abstract class ACommand
    {
        protected List<Layer> backup;

        public ACommand()
        {
            //De momento lo dejo así, pero el backup tendrá q ser de todas las capas
            backup = new List<Layer>();
        }

        public void SaveBackup()
        {
            var app = PaintEditorPlugin.Instance;
            foreach(var layer in app.canvas.layerList)
            {
                Rect r = new Rect(app.canvas.rect.position, app.canvas.realSize);
                Layer newLayer = new Layer(0, r);
                layer.CopyToLayer(newLayer);
                backup.Add(newLayer);
            }
        }

        public void Undo()
        {
            var app = PaintEditorPlugin.Instance;

            if(backup.Count < app.canvas.layerList.Count)
            {
                app.canvas.layerList.RemoveAt(app.canvas.layerList.Count - 1);
            }
            else if (backup.Count > app.canvas.layerList.Count)
            {
                Rect r = new Rect(app.canvas.rect.position, app.canvas.realSize);
                app.canvas.layerList.Add(new Layer(0, r));
            }

            for (int i = 0; i < backup.Count; i++)
            {
                var layer = backup[i];
                layer.CopyToLayer(app.canvas.layerList[i]);
            }
        }

        public abstract bool Execute();
    }

    public class CommandHistory
    {
        public LinkedList<ACommand> history { get; private set; }

        private static int limit = 100;

        public CommandHistory() 
        {
            history = new LinkedList<ACommand>();
        }

        public void Push(ACommand command)
        {
            if(history.Count >= limit)
            {
                history.RemoveFirst();
            }
            history.AddLast(command);
        }

        public ACommand Pop()
        {
            var item = history.Last.Value;
            history.RemoveLast();
            return item;
        }
    }
}

