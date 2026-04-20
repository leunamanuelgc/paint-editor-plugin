using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Pan : ITool
    {
        private const float minSpeed = .1f;
        private const float maxSpeed = 3f;

        public static string name = "Pan";
        public static string iconTextureName = "d_ViewToolMove";
        public static string tooltip = "Drag to move the canvas";
        public static GUIContent guiContent = new GUIContent((Texture)EditorGUIUtility.Load(iconTextureName), tooltip);

        public float speed { get; set; }

        protected string prefixLabelText { get; set; }

        public Pan(float speed)
        {
            prefixLabelText = "Pan speed";
            this.speed = speed;
        }

        public void Select()
        {
            DisplayOptionsGUI();
        }

        private void DisplayOptionsGUI()
        {
            EditorGUILayout.BeginHorizontal();
            speed = EditorGUILayout.Slider(new GUIContent(prefixLabelText), speed, minSpeed, maxSpeed);
            EditorGUILayout.EndHorizontal();
        }
    }
}