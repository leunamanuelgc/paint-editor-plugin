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

            EditorGUILayout.EndHorizontal();
        }

        public void SaveImage()
        {
            var path = EditorUtility.SaveFilePanelInProject("SaveImage", "new_image", "png", "Save Image");

            if (path.Length != 0)
            {
                byte[] bytes = PaintEditorPlugin.Instance.canvas.texture.EncodeToPNG();
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

            app.canvas.texture = new Texture2D(loadedTexture.width, loadedTexture.height, loadedTexture.format, true, false);
            Graphics.CopyTexture(loadedTexture, app.canvas.texture);
            app.canvas.texture.alphaIsTransparency = true;
            app.canvas.texture.filterMode = FilterMode.Point;

            app.canvas.size = new Vector2(app.canvas.texture.width, app.canvas.texture.height);
            app.canvas.rect = new Rect(app.canvas.position, app.canvas.size);
            app.canvas.aspectRatio = (float)app.canvas.texture.width / (float)app.canvas.texture.height;

            float newHeight = app.canvas.rect.width / app.canvas.aspectRatio;
            app.canvas.rect = new Rect(app.position.width / 2 - app.canvas.rect.width / 2, app.position.height / 2 - newHeight / 2, app.canvas.rect.width, newHeight);
        }

        public void CreateNewImageWindow()
        {
            EditorWindow.CreateWindow<NewImageWindow>();
        }
    }
}