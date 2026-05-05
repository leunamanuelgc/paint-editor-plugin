using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Pan : ITool
    {
        private const float minSpeed = .1f;
        private const float maxSpeed = 3f;
        private const string prefixSpeedLabel = "Pan speed";

        public static string name = "Pan";
        public static string iconTextureName = "d_ViewToolMove";
        public static string tooltip = "Drag to move the canvas";
        public static GUIContent guiContent = new GUIContent((Texture)EditorGUIUtility.Load(iconTextureName), tooltip);

        public float speed { get; set; }

        public Pan(float speed)
        {
            this.speed = speed;
        }

        public void Select()
        {
            DisplayOptionsGUI();
        }

        private void DisplayOptionsGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(guiContent, GUILayout.Width(20));
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(prefixSpeedLabel));
            EditorGUIUtility.labelWidth = textDimensions.x + 10;
            speed = EditorGUILayout.Slider(new GUIContent(prefixSpeedLabel), speed, minSpeed, maxSpeed);
            EditorGUILayout.EndHorizontal();
        }
    }
}