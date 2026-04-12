using UnityEditor;
using UnityEditor.PaintEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class ComputeTest : EditorWindow
{
    RenderTexture rT;
    Rect texturePos;

    const int resolution = 256;

    ComputeShader computeShader;

    private static readonly int textureId = Shader.PropertyToID("_T");
    private static readonly int resolutionId = Shader.PropertyToID("_Resolution");

    [MenuItem("Tools/ComputeTest")]
    public static void CreateComputeTest()
    {
        GetWindow<EditorWindow>();
        GetWindow(typeof(ComputeTest));
    }

    private void OnEnable()
    {
        computeShader = Resources.Load<ComputeShader>("ComputeTest");

        rT = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        rT.filterMode = FilterMode.Point;
        rT.enableRandomWrite = true;

        texturePos = new Rect(100, 100, resolution, resolution);

        Compute("FillTransparent");
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(100, 50, 50, 20), "Fill")) {
            Compute("FillWithRed");
        }

        EditorGUI.DrawTextureTransparent(texturePos, rT);
    }

    void Compute(string func)
    {
        int kernelId = computeShader.FindKernel(func);
        int groups = Mathf.CeilToInt(resolution / 8);
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetTexture(kernelId, textureId, rT);
        computeShader.Dispatch(kernelId, groups, groups, 1);
    }
}
