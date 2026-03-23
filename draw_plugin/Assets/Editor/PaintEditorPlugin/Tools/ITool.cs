using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public interface ITool
    {
        public void Use();

        public void Select();

        public void SetCommand(ACommand command);
    }
}

