using UnityEditorInternal;
using UnityEngine;
using static PaintEditorExtension.Layer;
using System;
using UnityEditor;

namespace PaintEditorExtension
{
    public class Utils : IToolbar
    {
        private float layersWidth, layersHeight, maxHeight;

        private const string layersText = "Color & Layers";
        private const string layersTooltip = "Change color and edit layers";

        private Vector2 scrollPos;

        public Rect layersRect { get; set; }
        public Rect infoRect { get; set; }
        public Color currentColor { get; set; }

        public ReorderableList layers;

        public static event Action onBlendModeChange;

        public Utils()
        {
            var app = PaintEditorExtension.Instance;
            layersWidth = 180;

            layers = new ReorderableList(app.canvas.layerList, typeof(Layer), true, true, true, true);
            layers.drawElementCallback = DrawLayers;
            layers.drawHeaderCallback = DrawLayersHeader;
            layers.onAddCallback = app.canvas.AddLayer;
            layers.onRemoveCallback = app.canvas.RemoveLayer;
            layers.onCanRemoveCallback = app.canvas.CanRemove;
            layers.onSelectCallback = app.canvas.SelectLayer;
            layers.elementHeightCallback = (int index) => EditorGUIUtility.singleLineHeight * 2 + 10;
            layers.onReorderCallback = ReorderCallback;
        }

        private void ReorderCallback(ReorderableList list)
        {
            PaintEditorExtension.Instance.ExecuteCommand(new ReorderLayerCommand());
        }

        public void DisplayGUI()
        {
            var app = PaintEditorExtension.Instance;

            layersHeight = Mathf.Min(maxHeight, layers.count * EditorGUIUtility.singleLineHeight * 2.7f + 130);
            layersRect = new Rect(app.position.width - (layersWidth + 20), 60, layersWidth, layersHeight);
            maxHeight = app.position.height - 120;
            GUIContent layersContent = new GUIContent(layersText, layersTooltip);
            GUIStyle layersStyle = new GUIStyle(GUI.skin.window);
            GUILayout.Window(1, layersRect, CreateLayersWindow, layersContent, layersStyle);

            infoRect = new Rect(0, app.position.height - 40, 250, 25);
            GUILayout.Window(2, infoRect, CreateInfoWindow, "");
        }

        private void CreateLayersWindow(int id)
        {
            string content = "Color";
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(content));
            EditorGUIUtility.labelWidth = textDimensions.x + 10;
            currentColor = EditorGUILayout.ColorField(new GUIContent(content), currentColor, false, true, false);

            if(GUIUtility.hotControl >= 200)
            {
                PaintEditorExtension.Instance.CancelClick(true);
            }

            EditorGUILayout.Space(20);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(layersRect.width));
            layers.DoLayoutList();
            EditorGUILayout.EndScrollView();

        }

        private void DrawLayers(Rect rect, int index, bool isActive, bool isFocused)
        {
            var app = PaintEditorExtension.Instance;
            var icon = app.canvas.layerList[index].isEnabled? Layer.iconTextureOn : Layer.iconTextureOff;

            if (GUI.Button(new Rect(rect.x, rect.y + 20, 25, 25), new GUIContent((Texture)EditorGUIUtility.Load(icon))))
            {
                var newValue = !app.canvas.layerList[index].isEnabled;
                app.canvas.layerList[index].isEnabled = newValue;
            }

            string[] blendModeOptions = Enum.GetNames(typeof(LayerBlendMode));
            var newBlendModeIdx = EditorGUI.Popup(new Rect(rect.x, rect.y, 130, EditorGUIUtility.singleLineHeight), app.canvas.layerList[index].blendModeIdx, blendModeOptions);
            if(newBlendModeIdx != app.canvas.layerList[index].blendModeIdx)
            {
                app.canvas.layerList[index].blendModeIdx = newBlendModeIdx;
                onBlendModeChange?.Invoke();
            }

            app.canvas.layerList[index].name = EditorGUI.TextField(new Rect(rect.x + 30, rect.y + 25, 100, EditorGUIUtility.singleLineHeight), app.canvas.layerList[index].name);
        }

        private void DrawLayersHeader(Rect rect)
        {
            string name = "Layers";
            EditorGUI.LabelField(rect, name);
        }

        private void CreateInfoWindow(int id)
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
            style.fontSize = 10;

            EditorGUILayout.BeginHorizontal();
            var app = PaintEditorExtension.Instance;
            string canvasInfo = $"Canvas: ({app.canvas.realSize.x},{app.canvas.realSize.y})";
            GUILayout.Label(canvasInfo, style, GUILayout.Width(100));

            var mousePos = app.GetCanvasMousePosition();
            string mouseInfo = $"Mouse Position: ({Mathf.RoundToInt(mousePos.x)},{Mathf.RoundToInt(mousePos.y)})";
            GUILayout.Label(mouseInfo, style, GUILayout.Width(140));
            
            var zoomLevel = app.GetZoomLevel() / app.GetBaseZoom();
            string zoomLevelInfo = $"Zoom Level: ({zoomLevel})";
            GUILayout.Label(zoomLevelInfo, style, GUILayout.Width(100));

            EditorGUILayout.EndHorizontal();
        }
    }
}