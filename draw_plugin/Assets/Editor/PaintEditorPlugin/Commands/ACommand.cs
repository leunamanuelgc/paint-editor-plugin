using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public abstract class ACommand
    {
        protected PaintEditorPlugin app;
        protected Texture2D backup;

        public ACommand(PaintEditorPlugin app)
        {
            this.app = app;
        }

        public void SaveBackup()
        {
            backup = app.canvas.texture;
        }

        public void Undo()
        {
            app.canvas.texture = backup;
        }

        public abstract void Execute();
    }
}

