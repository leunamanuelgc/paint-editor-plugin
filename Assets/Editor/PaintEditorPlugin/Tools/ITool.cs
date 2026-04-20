using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public interface ITool
    {
        public static string name;
        public static string iconTextureName;
        public static string tooltip;

        public void Select();
    }
}

