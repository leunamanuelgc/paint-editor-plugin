using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace PaintEditorExtension
{
    public class SaveCommand : ACommand
    {
        public SaveCommand() { }

        public override bool Execute()
        {
            var path = EditorUtility.SaveFilePanelInProject("SaveImage", "new_image", "png", "Save Image");

            if (path.Equals(""))
            {
                Debug.Log("Save cancelled: Empty path name");
                return false;
            }

            var app = PaintEditorExtension.Instance;
            RenderTexture temporaryResult = InitializeTexture((int)app.canvas.realSize.x, (int)app.canvas.realSize.y);
            if(app.layerSelection.selectionType == LayerSelection.SelectionType.edit ||
                app.layerSelection.selectionType == LayerSelection.SelectionType.transform)
            {
                app.ExecuteCommand(new MergeCommand(app.layerSelection, app.canvas.rect));
            }

            byte[] bytes = ApplyBlendMode(app.canvas.layerList, temporaryResult);
            
            File.WriteAllBytes(path, bytes);

            temporaryResult.Release();

            return false;
        }

        public RenderTexture InitializeTexture(int width, int height)
        {
            RenderTexture texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            texture.filterMode = FilterMode.Point;
            texture.enableRandomWrite = true;
            texture.Create();

            return texture;
        }

        public byte[] ApplyBlendMode(List<Layer> layerList, RenderTexture texture)
        {
            RenderTexture currentActive = RenderTexture.active;
            for (int i=layerList.Count - 1; i>=0; i--)
            {
                Graphics.Blit(layerList[i].rTexture, texture, layerList[i].blendMaterial);
            }

            Texture2D result = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
            RenderTexture.active = texture;

            result.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);

            RenderTexture.active = currentActive;
            return result.EncodeToPNG();   
        }
    }
}
