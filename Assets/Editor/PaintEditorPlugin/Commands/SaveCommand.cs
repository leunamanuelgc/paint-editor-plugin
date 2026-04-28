using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class SaveCommand : ACommand
    {
        private ComputeShader computeShader;

        private static readonly int textureSrcId = Shader.PropertyToID("_Source");
        private static readonly int textureDstId = Shader.PropertyToID("_Destination");
        private static readonly int resolutionId = Shader.PropertyToID("_Resolution");

        private static string computePath = PaintEditorPlugin.Instance.ComputePath() + "ComputeSave.compute";
        private static string computeFunc = "ComposeColors";

        public SaveCommand() { }

        public override bool Execute()
        {
            var path = EditorUtility.SaveFilePanelInProject("SaveImage", "new_image", "png", "Save Image");

            try
            {
                var app = PaintEditorPlugin.Instance;
                RenderTexture result = InitializeTexture((int)app.canvas.realSize.x, (int)app.canvas.realSize.y);
                byte[] bytes = ComputeSaveLayers(app.canvas.layerList, result);
                File.WriteAllBytes(path, bytes);
            }
            catch
            {
                Debug.Log($"Save cancelled: Empty path name");
            }

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

        private byte[] ComputeSaveLayers(List<Layer> layerList, RenderTexture rTextureResult)
        {
            computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(computePath);
            int kernelId = computeShader.FindKernel(computeFunc);
            int groups = Mathf.CeilToInt(rTextureResult.width / 8);
            Vector4 resolution = new Vector4(rTextureResult.width, rTextureResult.height);
            computeShader.SetVector(resolutionId, resolution);
            computeShader.SetTexture(kernelId, textureDstId, rTextureResult);

            for (int i = layerList.Count - 1; i >= 0 ; i--)
            {
                computeShader.SetTexture(kernelId, textureSrcId, layerList[i].rTexture);
                computeShader.Dispatch(kernelId, groups, groups, 1);
            }

            Texture2D result = new Texture2D(rTextureResult.width, rTextureResult.height, TextureFormat.ARGB32, false);
            RenderTexture.active = rTextureResult;
            result.ReadPixels(new Rect(0, 0, rTextureResult.width, rTextureResult.height), 0, 0);
            return result.EncodeToPNG();
        }
    }
}
