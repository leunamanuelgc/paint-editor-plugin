using System.IO;
using UnityEngine;
using UnityEditor;

namespace PaintEditorExtension
{
    public class LoadCommand : ACommand
    {
        public LoadCommand() { }

        public override bool Execute()
        {
            string[] extensionFiles = { "Image files", "png,jpg,jpeg", "All files", "*" };

            var selectedImage = EditorUtility.OpenFilePanelWithFilters("Load Image", Application.dataPath, extensionFiles);
                
            if (selectedImage.Equals(""))
            {
                Debug.Log("Load cancelled: Empty path name");
                return false;
            }

            var rawImageData = File.ReadAllBytes(selectedImage);

            Texture2D loadedTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            loadedTexture.alphaIsTransparency = true;
            ImageConversion.LoadImage(loadedTexture, rawImageData);

            var app = PaintEditorExtension.Instance;
            app.canvas.Load(loadedTexture);

            return false;
        }
    }
}
