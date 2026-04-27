using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public abstract class ACommand
    {
        protected RenderTexture backup;

        public ACommand()
        {
            var app = PaintEditorPlugin.Instance;
            //De momento lo dejo así, pero el backup tendrá q ser de todas las capas
            backup = new RenderTexture(app.canvas.selectedLayer.rTexture.width, app.canvas.selectedLayer.rTexture.height, 0,
                app.canvas.selectedLayer.rTexture.format, RenderTextureReadWrite.sRGB);
        }

        protected Vector2Int PosInRectInt(Vector2 pos, Rect rect)
        {
            float new_x = pos.x - rect.x;
            float new_y = rect.height - (pos.y - rect.y);

            return new Vector2Int((int)new_x, (int)new_y);
        }

        protected Vector2Int DeltaInt()
        {
            Vector2Int delta = new Vector2Int((int)Event.current.delta.x, (int)Event.current.delta.y);
            return delta;
        }

        protected Vector2 ConvertPos(Vector2 pos, Rect r, Vector2 size)
        {
            Vector2 convertion = new Vector2(size.x / r.width, size.y / r.height);

            return new Vector2(pos.x * convertion.x, pos.y * convertion.y);
        }

        public void SaveBackup()
        {
            Graphics.CopyTexture(PaintEditorPlugin.Instance.canvas.selectedLayer.rTexture, backup);
        }

        public void Undo()
        {
            Graphics.CopyTexture(backup, PaintEditorPlugin.Instance.canvas.selectedLayer.rTexture);
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

