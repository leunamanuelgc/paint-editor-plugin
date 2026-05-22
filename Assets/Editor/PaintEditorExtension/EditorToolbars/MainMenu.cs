using UnityEngine;
using UnityEditor;

namespace PaintEditorExtension
{
    public class MainMenu : IToolbar
    {
        public MainMenu() { }

        public void DisplayGUI()
        {
            EditorGUILayout.BeginHorizontal();

            if (EditorGUILayout.DropdownButton(new GUIContent("File"), FocusType.Keyboard, EditorStyles.toolbarButton))
            {
                PaintEditorExtension.Instance.CancelClick(true);

                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("New"), false, CreateNewImageWindow);

                menu.AddItem(new GUIContent("Save"), false, SaveImage);

                menu.AddItem(new GUIContent("Load"), false, LoadImage);

                menu.ShowAsContext();
            }

            if (EditorGUILayout.DropdownButton(new GUIContent("Edit"), FocusType.Keyboard, EditorStyles.toolbarButton))
            {
                PaintEditorExtension.Instance.CancelClick(true);

                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Undo"), false, PaintEditorExtension.Instance.Undo);

                menu.AddItem(new GUIContent("Redo"), false, PaintEditorExtension.Instance.Redo);

                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();
        }

        public void SaveImage()
        {
            PaintEditorExtension.Instance.ExecuteCommand(new SaveCommand());
        }

        public void LoadImage()
        {
            PaintEditorExtension.Instance.ExecuteCommand(new LoadCommand());
        }

        public void CreateNewImageWindow()
        {
            EditorWindow.CreateWindow<NewImageWindow>();
        }
    }
}