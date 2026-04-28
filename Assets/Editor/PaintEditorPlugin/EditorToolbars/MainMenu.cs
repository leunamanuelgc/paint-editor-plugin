using UnityEngine;
using System.IO;

namespace UnityEditor.PaintEditor
{
    public class MainMenu : IToolbar
    {
        public MainMenu() { }

        public void DisplayGUI()
        {
            EditorGUILayout.BeginHorizontal();

            if (EditorGUILayout.DropdownButton(new GUIContent("File"), FocusType.Keyboard, EditorStyles.toolbarButton))
            {
                PaintEditorPlugin.Instance.CancelClick(true);

                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("New"), false, CreateNewImageWindow);

                menu.AddItem(new GUIContent("Save"), false, SaveImage);

                menu.AddItem(new GUIContent("Load"), false, LoadImage);

                menu.ShowAsContext();
            }

            if (EditorGUILayout.DropdownButton(new GUIContent("Edit"), FocusType.Keyboard, EditorStyles.toolbarButton))
            {
                PaintEditorPlugin.Instance.CancelClick(true);

                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Undo"), false, PaintEditorPlugin.Instance.Undo);

                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();
        }

        public void SaveImage()
        {
            PaintEditorPlugin.Instance.ExecuteCommand(new SaveCommand());
        }

        public void LoadImage()
        {
            PaintEditorPlugin.Instance.ExecuteCommand(new LoadCommand());
        }

        public void CreateNewImageWindow()
        {
            EditorWindow.CreateWindow<NewImageWindow>();
        }
    }
}