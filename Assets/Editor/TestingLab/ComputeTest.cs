using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class ComputeTest : EditorWindow
{
    int id = 0;
    Rect texturePos;
    const int resolution = 8192;
    Vector2Int size;
    Color col;

    #region Compute
    struct BrushData
    {
        public Vector2Int pos;
        public Vector2Int size;
        public Color color;
    };

    RenderTexture rT;
    
    ComputeShader computeShader;
    ComputeBuffer buffer;
    BrushData[] data;

    private static readonly int textureId = Shader.PropertyToID("_Texture");
    private static readonly int resolutionId = Shader.PropertyToID("_Resolution");
    private static readonly int brushBufferId = Shader.PropertyToID("_BrushBuffer");

    #endregion

    #region Texture2D

    Texture2D t;

    #endregion

    [MenuItem("Tools/ComputeTest")]
    public static void CreateComputeTest()
    {
        GetWindow<EditorWindow>();
        GetWindow(typeof(ComputeTest));
    }

    private void OnEnable()
    {
        texturePos = new Rect(100, 100, resolution, resolution);
        id = 0;
        size = new Vector2Int(1, 1);
        col = Color.black;

        InitializeCompute();
    }

    void InitializeCompute()
    {
        computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Editor/TestingLab/ComputeTest.compute");

        rT = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        rT.filterMode = FilterMode.Point;
        rT.enableRandomWrite = true;
        
        buffer = new ComputeBuffer(1, Marshal.SizeOf<BrushData>());
        data = new BrushData[1];
        data[0].color = col;
        data[0].pos = new Vector2Int(0, 0);
        data[0].size = size;
    }

    void ClearCompute()
    {
        DestroyImmediate(rT);
        buffer.Dispose();
        buffer = null;
        data = null;
    }

    void InitializeTexture2D()
    {
        t = new Texture2D(resolution, resolution);
        t.filterMode = FilterMode.Point;

        Color[] transparent = new Color[resolution * resolution];
        Array.Fill(transparent, new Color(0, 0, 0, 0));
        t.SetPixels(transparent);
        t.Apply();
    }

    void ClearTexture2D()
    {
        DestroyImmediate(t);
    }

    private void OnDisable()
    {
        buffer.Release();
        buffer = null;
    }

    private void OnGUI()
    {
        Vector2 textDimensions = GUI.skin.label.CalcSize(new GUIContent("X"));
        EditorGUIUtility.labelWidth = textDimensions.x + 10;

        EditorGUILayout.BeginHorizontal();
        string[] popupOptions = { "Compute", "Texture2D" };
        int newId = EditorGUILayout.Popup(id, popupOptions);

        if(newId != id)
        {
            SetId(newId);
        }

        size.x = EditorGUILayout.IntSlider("X",size.x, 1, 100, GUILayout.MinWidth(100));
        size.y = EditorGUILayout.IntSlider("Y",size.y, 1, 100, GUILayout.MinWidth(100));
        col = EditorGUILayout.ColorField(col);
        EditorGUILayout.EndHorizontal();
        
        if (id == 0)
        {
            ComputeDraw();
        }
        else
        {
            Texture2DDraw();
        }
    }

    void SetId(int newId)
    {
        if (id == 0)
        {
            ClearCompute();
            InitializeTexture2D();
        }
        else
        {
            ClearTexture2D();
            InitializeCompute();
        }

        id = newId;
    }

    void ComputeDraw()
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag && texturePos.Contains(e.mousePosition))
        {
            Vector2 pos = PosInRect(e.mousePosition, texturePos);
            DispatchPaint(pos);
            //Repaint();
        }

        EditorGUI.DrawTextureTransparent(texturePos, rT);
    }

    void DispatchPaint(Vector2 mousePosition)
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

    void Texture2DDraw()
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag && texturePos.Contains(e.mousePosition))
        {
            Vector2 pos = PosInRect(e.mousePosition, texturePos);
            TexturePaint(pos);
            //Repaint();
        }

        EditorGUI.DrawTextureTransparent(texturePos, t);
    }

    void TexturePaint(Vector2 pos)
    {
        Color[] pixels = new Color[size.x * size.y];
        Array.Fill(pixels, col);
        t.SetPixels((int)pos.x, (int)pos.y, size.x, size.y, pixels);
        t.Apply();
    }

    public Vector2 PosInRect(Vector2 pos, Rect rect)
    {
        float new_x = pos.x - rect.x;
        float new_y = rect.height - (pos.y - rect.y);

        return new Vector2(new_x, new_y);
    }
}
