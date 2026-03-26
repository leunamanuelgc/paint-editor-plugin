using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Pan : ITool
    {
        private const float minSpeed = .1f;
        private const float maxSpeed = 3f;

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
            speed = EditorGUILayout.Slider(new GUIContent(prefixLabelText), speed, minSpeed, maxSpeed);
        }

        public void MoveCanvas()
        {

        }
    }
}