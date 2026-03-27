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
            backup = new Texture2D(app.canvas.texture.width, app.canvas.texture.height, app.canvas.texture.format, true);
        }

        public void SaveBackup()
        {
            Graphics.CopyTexture(PaintEditorPlugin.Instance.canvas.texture, backup);
        }

        public void Undo()
        {
            Graphics.CopyTexture(backup, PaintEditorPlugin.Instance.canvas.texture);
            PaintEditorPlugin.Instance.Repaint();
        }

        public abstract bool Execute();
    }

    public class CommandHistory
    {
        private Stack<ACommand> history;

        public CommandHistory() 
        {
            history = new Stack<ACommand>();
        }

        public void Push(ACommand command)
        {
            history.Push(command);
        }

        public ACommand Pop()
        {
            return history.Pop();
        }
    }
}

