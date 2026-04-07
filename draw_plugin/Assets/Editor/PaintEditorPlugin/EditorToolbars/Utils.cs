using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

namespace UnityEditor.PaintEditor
{
    public class Utils : IToolbar
    {
        private Rect rect;
        private float width, height;

        private const string text = "Utils";
        private const string tooltip = "Change color";

        public Color currentColor { get; set; }

        public ReorderableList layers;

        internal List<Layer> layerList { get; set; }

        public Layer selectedLayer { get; set; }

        public Utils()
        {
            var app = PaintEditorPlugin.Instance;
            width = 200;
            height = 300;
            rect = new Rect(app.position.width - (width + 20), 100, width, height);

            layerList = new List<Layer>() { new Layer(0) };
            selectedLayer = layerList[0];

            layers = new ReorderableList(layerList, typeof(Layer), true, true, true, true);
            layers.drawElementCallback = DrawLayers;
            layers.drawHeaderCallback = DrawHeader;
            layers.onAddCallback = AddLayer;
            layers.onRemoveCallback = RemoveLayer;
            layers.elementHeightCallback = (int index) => EditorGUIUtility.singleLineHeight + 10;
            layers.onSelectCallback = SelectLayer;
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
            currentColor = EditorGUILayout.ColorField(new GUIContent("Color"), currentColor, true, true, true);

            EditorGUILayout.Space(20);

            layers.DoLayoutList();
        }

        private void DrawLayers(Rect rect, int index, bool isActive, bool isFocused)
        {
            var icon = layerList[index].isEnabled? Layer.iconTextureOn : Layer.iconTextureOff;

            if (GUI.Button(new Rect(rect.x, rect.y, 25, 25), new GUIContent((Texture)EditorGUIUtility.Load(icon))))
            {
                var newValue = !layerList[index].isEnabled;
                layerList[index].isEnabled = newValue;

                if (newValue == false)
                {
                    //Hide texture (somehow). I think I might have to encode the data to save it and then restore it when it gets back to true or something.
                    //layerList[index].texture 
                }
            }
            
            layerList[index].name = EditorGUI.TextField(new Rect(rect.x + 30, rect.y + 4, 100, EditorGUIUtility.singleLineHeight), layerList[index].name);
        }

        private void DrawHeader(Rect rect)
        {
            string name = "Layers";
            EditorGUI.LabelField(rect, name);
        }

        private void AddLayer(ReorderableList list)
        {
            layerList.Add(new Layer(list.count));
        }

        private void RemoveLayer(ReorderableList list)
        {
            layerList.RemoveAt(layerList.Count - 1);
        }

        private void SelectLayer(ReorderableList list)
        {
            selectedLayer = layerList[list.index];
        }
    }
}