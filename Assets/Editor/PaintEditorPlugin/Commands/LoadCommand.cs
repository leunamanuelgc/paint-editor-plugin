using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class LoadCommand : ACommand
    {
        public LoadCommand() { }

        public override bool Execute()
        {
            try
            {
                string[] extensionFiles = { "Image files", "png,jpg,jpeg", "All files", "*" };
                var selectedImage = EditorUtility.OpenFilePanelWithFilters("Load Image", Application.dataPath, extensionFiles);

                var rawImageData = File.ReadAllBytes(selectedImage);

                Texture2D loadedTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                loadedTexture.alphaIsTransparency = true;
                ImageConversion.LoadImage(loadedTexture, rawImageData);

                var app = PaintEditorPlugin.Instance;
                app.canvas.Load(loadedTexture);
            }
            catch
            {
                Debug.Log("Load cancelled: Empty path name");
                return false;
            }
            return false;
        }
    }
}
