using UnityEditor;

namespace PaintEditorExtension
{
    public class EditorSingleton<T> : EditorWindow where T : EditorWindow
    {
        public static T Instance { get; private set; }

        protected virtual void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
            else
            {
                Close();
            }
        }
    }
}
