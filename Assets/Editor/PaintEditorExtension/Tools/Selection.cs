using System;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class Selection : ITool
    {
        public static string name = "Selection";
        public static string iconTextureName = "d_Grid.BoxTool";
        public static string tooltip = "Drag to select sections of a layer";
        public static GUIContent guiContent = new GUIContent((Texture)EditorGUIUtility.Load(iconTextureName), tooltip);
        public static event Action OnTransformMode;

        public Selection() { }

        public void Select()
        {
            DisplayOptionsGUI();
        }

        private void DisplayOptionsGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(guiContent, GUILayout.Width(20));

            if (PaintEditorPlugin.Instance.layerSelection.selectionType == LayerSelection.SelectionType.edit)
            {
                if (GUILayout.Button("Transform", GUILayout.Width(100)))
                {
                    OnTransformMode?.Invoke();
                }
            }
            else if (PaintEditorPlugin.Instance.layerSelection.selectionType == LayerSelection.SelectionType.transform)
            {
                GUILayout.Box("Transform", GUILayout.Width(100));

                if (GUILayout.Button("Apply", GUILayout.Width(100)))
                {
                    PaintEditorPlugin.Instance.CloseSelection();
                }
            }
            else
            {
                GUILayout.Box("Transform", GUILayout.Width(100));
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}