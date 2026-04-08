using UnityEngine;
using UnityEditor;
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
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("New"), true, CreateNewImageWindow);

                menu.AddItem(new GUIContent("Save"), true, SaveImage);

                menu.AddItem(new GUIContent("Load"), true, LoadImage);

                menu.ShowAsContext();
            }

            if (EditorGUILayout.DropdownButton(new GUIContent("Edit"), FocusType.Keyboard, EditorStyles.toolbarButton))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Undo"), true, PaintEditorPlugin.Instance.Undo);

                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();
        }

        public void SaveImage()
        {
            var path = EditorUtility.SaveFilePanelInProject("SaveImage", "new_image", "png", "Save Image");

            if (path.Length != 0)
            {
                //Guardar imagen tendrá que codificar todas las capas en una sola. Además, debería mover esta lógica al Canvas. De momento lo dejo así
                byte[] bytes = PaintEditorPlugin.Instance.canvas.selectedLayer.texture.EncodeToPNG();
                File.WriteAllBytes(path, bytes);
            }
        }

        public void LoadImage()
        {
            string[] extensionFiles = { "Image files", "png,jpg,jpeg", "All files", "*" };
            var selectedImage = EditorUtility.OpenFilePanelWithFilters("Load Image", Application.dataPath, extensionFiles);

            var rawImageData = File.ReadAllBytes(selectedImage);

            Texture2D loadedTexture = new Texture2D(1, 1);
            ImageConversion.LoadImage(loadedTexture, rawImageData);

            var app = PaintEditorPlugin.Instance;
            app.canvas.Load(loadedTexture);
        }

        public void CreateNewImageWindow()
        {
            EditorWindow.CreateWindow<NewImageWindow>();
        }
    }
}