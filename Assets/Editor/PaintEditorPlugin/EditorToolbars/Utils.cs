using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

namespace UnityEditor.PaintEditor
{
    public class Utils : IToolbar
    {
        private float width, height;

        private const string text = "Utils";
        private const string tooltip = "Change color";

        public Rect rect { get; set; }
        public Color currentColor { get; set; }

        public ReorderableList layers;

        public Utils()
        {
            var app = PaintEditorPlugin.Instance;
            width = 200;
            height = 300;
            rect = new Rect(app.position.width - (width + 20), 100, width, height);

            layers = new ReorderableList(app.canvas.layerList, typeof(Layer), true, true, true, true);
            layers.drawElementCallback = DrawLayers;
            layers.drawHeaderCallback = DrawHeader;
            layers.onAddCallback = app.canvas.AddLayer;
            layers.onRemoveCallback = app.canvas.RemoveLayer;
            layers.onCanRemoveCallback = app.canvas.CanRemove;
            layers.onSelectCallback = app.canvas.SelectLayer;
            layers.elementHeightCallback = (int index) => EditorGUIUtility.singleLineHeight + 10;
            layers.onReorderCallback = ReorderCallback;
        }

        private void ReorderCallback(ReorderableList list)
        {
            PaintEditorPlugin.Instance.ExecuteCommand(new ReorderLayerCommand());
        }

        public void DisplayGUI()
        {
            var app = PaintEditorPlugin.Instance;

            rect = new Rect(app.position.width - (width + 20), 100, width, height);
            GUIContent content = new GUIContent(text, tooltip);
            GUIStyle style = new GUIStyle(GUI.skin.window);

            GUILayout.Window(1, rect, CreateWindow, content, style);
        }

        private void CreateWindow(int id)
        {
            string content = "Color";
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(content));
            EditorGUIUtility.labelWidth = textDimensions.x + 10;
            currentColor = EditorGUILayout.ColorField(new GUIContent(content), currentColor, false, true, true);

            if(GUIUtility.hotControl >= 200)
            {
                PaintEditorPlugin.Instance.CancelClick(true);
            }

            EditorGUILayout.Space(20);

            layers.DoLayoutList();
        }

        private void DrawLayers(Rect rect, int index, bool isActive, bool isFocused)
        {
            var app = PaintEditorPlugin.Instance;
            var icon = app.canvas.layerList[index].isEnabled? Layer.iconTextureOn : Layer.iconTextureOff;

            if (GUI.Button(new Rect(rect.x, rect.y, 25, 25), new GUIContent((Texture)EditorGUIUtility.Load(icon))))
            {
                var newValue = !app.canvas.layerList[index].isEnabled;
                app.canvas.layerList[index].isEnabled = newValue;
            }

            app.canvas.layerList[index].name = EditorGUI.TextField(new Rect(rect.x + 30, rect.y + 4, 100, EditorGUIUtility.singleLineHeight), app.canvas.layerList[index].name);
        }

        private void DrawHeader(Rect rect)
        {
            string name = "Layers";
            EditorGUI.LabelField(rect, name);
        }
    }
}