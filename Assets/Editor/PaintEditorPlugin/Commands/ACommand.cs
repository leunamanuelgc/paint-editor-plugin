using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public abstract class ACommand
    {
        //protected RenderTexture backup;

        public ACommand()
        {
            var app = PaintEditorPlugin.Instance;
            //De momento lo dejo así, pero el backup tendrá q ser de todas las capas
            //backup = new RenderTexture(app.canvas.selectedLayer.rTexture.width, app.canvas.selectedLayer.rTexture.height, 0,
                //app.canvas.selectedLayer.rTexture.format, RenderTextureReadWrite.sRGB);
        }

        public void SaveBackup()
        {
            //Graphics.CopyTexture(PaintEditorPlugin.Instance.canvas.selectedLayer.rTexture, backup);
        }

        public void Undo()
        {
            //Graphics.CopyTexture(backup, PaintEditorPlugin.Instance.canvas.selectedLayer.rTexture);
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

