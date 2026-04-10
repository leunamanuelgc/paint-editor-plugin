using UnityEditor;
using UnityEditor.PaintEditor;
using UnityEngine;

public class ComputeTest : EditorWindow
{
    Texture2D texture;
    Rect texturePos;

    [MenuItem("Tools/ComputeTest")]
    public static void CreateComputeTest()
    {
        GetWindow<EditorWindow>();
        GetWindow(typeof(ComputeTest));
    }

    private void OnEnable()
    {
        texture = new Texture2D(256, 256, TextureFormat.ARGB32, true);
        texture.alphaIsTransparency = true;
        texture.filterMode = FilterMode.Point;

        texturePos = new Rect(100, 100, 256, 256);
    }

    private void OnGUI()
    {
        GUI.DrawTexture(texturePos, texture);
    }
}
