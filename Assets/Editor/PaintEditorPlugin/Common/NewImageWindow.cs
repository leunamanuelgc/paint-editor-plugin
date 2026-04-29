using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class NewImageWindow : EditorWindow
    {
        private int width { get; set; }

        private int height { get; set; }

        public static void Init()
        {
            GetWindow(typeof(NewImageWindow));
        }

        private void OnEnable()
        {
            width = height = 256;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            width = EditorGUILayout.IntField(new GUIContent("Width"), width);

            height = EditorGUILayout.IntField(new GUIContent("Height"), height);

            if (GUILayout.Button(new GUIContent("Create")))
            {
                PaintEditorPlugin.Instance.canvas.Reinitialize(new Vector2(width, height));

                Close();
            }

            EditorGUILayout.EndVertical();
        }
    }
}

