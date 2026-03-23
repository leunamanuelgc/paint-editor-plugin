using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public abstract class AEditorToolbar
    {
        protected PaintEditorPlugin app { get; private set; }

        public AEditorToolbar(PaintEditorPlugin app)
        {
            this.app = app;
        }
    }
}

