using PlasticGui.WorkspaceWindow.QueryViews.Branches;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.PaintEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class ComputeTest : EditorWindow
{
    struct BrushData
    {
        public Vector2Int pos;
        public Vector2Int size;
        public Color color;
    };

    RenderTexture rT;
    Rect texturePos;

    const int resolution = 256;

    Vector2Int size;
    Color col;

    ComputeShader computeShader;
    ComputeBuffer buffer;
    BrushData[] data;

    private static readonly int textureId = Shader.PropertyToID("_Texture");
    private static readonly int resolutionId = Shader.PropertyToID("_Resolution");
    private static readonly int brushBufferId = Shader.PropertyToID("_BrushBuffer");
    

    [MenuItem("Tools/ComputeTest")]
    public static void CreateComputeTest()
    {
        GetWindow<EditorWindow>();
        GetWindow(typeof(ComputeTest));
    }

    private void OnEnable()
    {
        computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Editor/TestingLab/ComputeTest.compute");

        rT = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        rT.filterMode = FilterMode.Point;
        rT.enableRandomWrite = true;

        texturePos = new Rect(100, 100, resolution, resolution);

        buffer = new ComputeBuffer(1, Marshal.SizeOf<BrushData>());
        data = new BrushData[1];
        data[0].color = Color.black;
        data[0].pos = new Vector2Int(0, 0);
        data[0].size = new Vector2Int(1, 1);

        size = new Vector2Int(1, 1);
        col = Color.black;
    }

    private void OnGUI()
    {
        size.x = EditorGUI.IntSlider(new Rect(50, 50, 150, 20), size.x, 0, 20);
        size.y = EditorGUI.IntSlider(new Rect(210, 50, 150, 20), size.y, 0, 20);
        col = EditorGUI.ColorField(new Rect(360, 50, 50, 20), col);

        Event e = Event.current;

        if(e.type == EventType.MouseDown || e.type == EventType.MouseDrag && texturePos.Contains(e.mousePosition))
        {
            Vector2 pos = PosInRect(e.mousePosition, texturePos);
            DrawTexture(pos);
            Repaint();
        }
        
        EditorGUI.DrawTextureTransparent(texturePos, rT);
    }

    void DrawTexture(Vector2 mousePosition)
    {
        int kernelId = computeShader.FindKernel("Draw");
        int groups = Mathf.CeilToInt(resolution / 8);
        computeShader.SetTexture(kernelId, textureId, rT);
        computeShader.SetInt(resolutionId, resolution);

        data[0].pos = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);
        data[0].color = col;
        data[0].size = size;
        buffer.SetData(data);
        computeShader.SetBuffer(kernelId, brushBufferId, buffer);
    
        computeShader.Dispatch(kernelId, groups, groups, 1);
    }

    public Vector2 PosInRect(Vector2 pos, Rect rect)
    {
        float new_x = pos.x - rect.x;
        float new_y = rect.height - (pos.y - rect.y);

        return new Vector2(new_x, new_y);
    }
}
