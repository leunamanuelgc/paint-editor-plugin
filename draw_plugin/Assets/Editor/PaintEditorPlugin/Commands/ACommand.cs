using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public abstract class ACommand
    {
        protected Texture2D backup;

        public ACommand()
        {
            var app = PaintEditorPlugin.Instance;
            //De momento lo dejo así, pero el backup tendrá q ser de todas las capas seguramente
            backup = new Texture2D(app.canvas.selectedLayer.texture.width, app.canvas.selectedLayer.texture.height, app.canvas.selectedLayer.texture.format, true);
        }

        public void SaveBackup()
        {
            Graphics.CopyTexture(PaintEditorPlugin.Instance.canvas.selectedLayer.texture, backup);
        }

        public void Undo()
        {
            Graphics.CopyTexture(backup, PaintEditorPlugin.Instance.canvas.selectedLayer.texture);
            PaintEditorPlugin.Instance.Repaint();
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

