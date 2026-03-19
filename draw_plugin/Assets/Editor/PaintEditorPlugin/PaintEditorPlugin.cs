using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using System;


public class PaintEditorPlugin : EditorWindow
{
    PaintCanvas canvas;

    enum ToolType
    {
        select,
        brush,
        eraser,
    }

    ToolType current_tool;

    Color current_color;
    
    enum BrushShapeType
    {
        box,
        rect,
    }

    int min_brush_size, max_brush_size;
    Vector2Int brush_vector_size;
    int brush_type_index;
    

    [MenuItem("Tools/Raster Editor")]
    public static void CreateEditorWindow()
    {
        GetWindow<EditorWindow>();
        GetWindow(typeof(PaintEditorPlugin));
    }

    public void OnEnable()
    {
        Vector2 canvasPosition = new Vector2(this.position.width / 2, this.position.height / 2);
        Rect rect = new Rect(canvasPosition.x, canvasPosition.y, 256, 256);
        Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, true, false);
        texture.alphaIsTransparency = true;
        Color[] colors = new Color[(int)rect.width * (int)rect.height];
        Array.Fill(colors, new Color(0, 0, 0, 0));
        texture.SetPixels(0, 0, (int)rect.width, (int)rect.height, colors);
        texture.Apply();

        canvas = new PaintCanvas(rect, texture);

        current_color = Color.black;

        min_brush_size = 1;
        max_brush_size = 100;
        brush_vector_size = new Vector2Int(1, 1);
        brush_type_index = 0;

        current_tool = ToolType.brush;

        Repaint();
    }

    public void OnGUI()
    {
        Debug.Log(position);
        EditorGUILayout.BeginVertical();

        displayMainMenuToolbar();

        displayOptionsToolbar(current_tool);

        EditorGUILayout.Space(15);

        EditorGUILayout.BeginHorizontal();

        displayToolboxToolbar();

        EditorGUILayout.Space(400);

        displayCanvas();

        displayFunctionsToolbar();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    void displayMainMenuToolbar()
    {
        EditorGUILayout.BeginHorizontal();

        if (EditorGUILayout.DropdownButton(new GUIContent("File"), FocusType.Keyboard, EditorStyles.toolbarButton))
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Save"), true, saveImage);

            menu.AddItem(new GUIContent("Load"), true, loadImage);

            menu.AddItem(new GUIContent("Test/1"), true, test, 1);

            menu.AddItem(new GUIContent("Test/2"), true, test, 2);

            menu.ShowAsContext();
        }

        if (EditorGUILayout.DropdownButton(new GUIContent("Edit"), FocusType.Keyboard, EditorStyles.toolbarButton))
        {
            GenericMenu menu = new GenericMenu();

            menu.ShowAsContext();
        }

        if (EditorGUILayout.DropdownButton(new GUIContent("Image"), FocusType.Keyboard, EditorStyles.toolbarButton))
        {
            GenericMenu menu = new GenericMenu();
                
            menu.ShowAsContext();
        }

        if (EditorGUILayout.DropdownButton(new GUIContent("Selection"), FocusType.Keyboard, EditorStyles.toolbarButton))
        {
            GenericMenu menu = new GenericMenu();

            menu.ShowAsContext();
        }

        if (EditorGUILayout.DropdownButton(new GUIContent("Help"), FocusType.Keyboard, EditorStyles.toolbarButton))
        {
            GenericMenu menu = new GenericMenu();

            menu.ShowAsContext();
        }

        EditorGUILayout.EndHorizontal();
    }

    void displayOptionsToolbar(ToolType toolSelected)
    {
        EditorGUILayout.BeginHorizontal();

        switch (toolSelected)
        {
            case ToolType.select:
                selectToolOptions();
                break;
            case ToolType.brush:
                brushToolOptions();
                break;
            case ToolType.eraser:
                eraserToolOptions();
                break;
        }

        EditorGUILayout.EndHorizontal();
    }

    void selectToolOptions()
    {
        if (EditorGUILayout.DropdownButton(new GUIContent("Option 1"), FocusType.Keyboard, EditorStyles.toolbarButton))
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Test/1"), true, test, 1);

            menu.ShowAsContext();
        }

        string[] optionsTest = new string[] { "A", "B", "C" };
        int index = 0;

        index = EditorGUILayout.Popup(index, optionsTest);
    }

    void brushToolOptions()
    {
        string[] brushTypeOptionsList = { BrushShapeType.box.ToString(), BrushShapeType.rect.ToString() };
        brush_type_index = EditorGUILayout.Popup(brush_type_index, brushTypeOptionsList);

        EditorGUILayout.PrefixLabel("Brush size");

        switch (brush_type_index)
        {
            case 0:
                int brush_size = EditorGUILayout.IntSlider(new GUIContent(""), brush_vector_size.x, min_brush_size, max_brush_size);
                brush_vector_size = new Vector2Int(brush_size, brush_size);
                break;
            case 1:
                int brush_size_x = EditorGUILayout.IntSlider(new GUIContent("X"), brush_vector_size.x, min_brush_size, max_brush_size);
                int brush_size_y = EditorGUILayout.IntSlider(new GUIContent("Y"), brush_vector_size.y, min_brush_size, max_brush_size);
                brush_vector_size = new Vector2Int(brush_size_x, brush_size_y);
                break;
        }
    }

    void eraserToolOptions()
    {
        string[] brushTypeOptionsList = { BrushShapeType.box.ToString(), BrushShapeType.rect.ToString() };
        brush_type_index = EditorGUILayout.Popup(brush_type_index, brushTypeOptionsList);

        EditorGUILayout.PrefixLabel("Eraser size");

        switch (brush_type_index)
        {
            case 0:
                int brush_size = EditorGUILayout.IntSlider(new GUIContent(""), brush_vector_size.x, min_brush_size, max_brush_size);
                brush_vector_size = new Vector2Int(brush_size, brush_size);
                break;
            case 1:
                int brush_size_x = EditorGUILayout.IntSlider(new GUIContent("X"), brush_vector_size.x, min_brush_size, max_brush_size);
                int brush_size_y = EditorGUILayout.IntSlider(new GUIContent("Y"), brush_vector_size.y, min_brush_size, max_brush_size);
                brush_vector_size = new Vector2Int(brush_size_x, brush_size_y);
                break;
        }
    }

    void displayToolboxToolbar()
    {
        EditorGUILayout.BeginVertical();

        if (EditorGUILayout.DropdownButton(new GUIContent("select"), FocusType.Keyboard, EditorStyles.toolbarButton))
        {
            var rect = GUILayoutUtility.GetLastRect();

            current_tool = ToolType.select;
        }

        if (EditorGUILayout.DropdownButton(new GUIContent("brush"), FocusType.Keyboard, EditorStyles.toolbarButton))
        {
            current_tool = ToolType.brush;
        }

        if (EditorGUILayout.DropdownButton(new GUIContent("eraser"), FocusType.Keyboard, EditorStyles.toolbarButton))
        {
            current_tool = ToolType.eraser;
        }

        EditorGUILayout.EndVertical();
    }

    void displayCanvas()
    {

        EditorGUI.DrawTextureTransparent(canvas.rect, canvas.texture, ScaleMode.ScaleToFit, canvas.texture.width / canvas.texture.height);

        if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
        {
            //canvas.Rect.Contains(Event.current.mousePosition) &&
            if ( current_tool == ToolType.brush)
            {
                PaintInCanvas(current_color);
            }
            else if (current_tool == ToolType.eraser)
            {
                Color eraserColor = new Color(0, 0, 0, 0);
                PaintInCanvas(eraserColor);
            }
        }
    }

    void PaintInCanvas(Color color)
    {
        float xPosCanvas = Event.current.mousePosition.x - canvas.rect.x;
        float yPosCanvas = canvas.rect.height - (Event.current.mousePosition.y - canvas.rect.y);

        Vector2 delta = Event.current.delta;
        
        float prev_xPosCanvas = xPosCanvas - delta.x;
        float prev_yPosCanvas = yPosCanvas + delta.y;

        float x_canvas_convertion = canvas.texture.width / canvas.rect.width;
        float y_canvas_convertion = canvas.texture.height / canvas.rect.height;

        float x1 = xPosCanvas * x_canvas_convertion;
        float y1 = yPosCanvas * y_canvas_convertion;
        float x0 = prev_xPosCanvas * x_canvas_convertion;
        float y0 = prev_yPosCanvas * y_canvas_convertion;

        float x = x0;
        float y = y0;
        float deltaX = (x1 - x0);
        float deltaY = (y1 - y0);
        float mY = deltaY / deltaX;
        float mX = deltaX / deltaY;

        //Debug.Log("x0: " + x0 + ", y0: " + y0 + ", x: " + x + ", y: " + y + ", x1: " + x1 + ", y1: " + y1);

        while (x != x1 || y != y1)
        {
            if (x0 < x1 && x > x1)
            {
                if (y0 < y1 && y > y1)
                {
                    break;
                }
                else if (y0 > y1 && y < y1)
                {
                    break;
                }
            } else if (x0 > x1 && x < x1)
            {
                if (y0 < y1 && y > y1)
                {
                    break;
                }
                else if (y0 > y1 && y < y1)
                {
                    break;
                }
            }

            PaintPixels(color, x, y, brush_vector_size.x, brush_vector_size.y);
            // I'm using Bresenham's line algorithm to fill gaps between painting frames
            if (Mathf.Abs(deltaX) >= Mathf.Abs(deltaY))
            {
                if (x0 != x1)
                    x += x0 < x1 ? 1 : -1;
                y = mY * (x - x0) + y0;
            }
            else
            {
                if (y0 != y1)
                    y += y0 < y1 ? 1 : -1;
                x = mX * (y - y0) + x0;
            }
        }

        PaintPixels(color, x, y, brush_vector_size.x, brush_vector_size.y);
        canvas.texture.Apply();
        Repaint();
    }

    void PaintPixels(Color color, float x, float y, int sizeX, int sizeY)
    {
        Rect point = new Rect(x - sizeX / 2, y - sizeY / 2, sizeX, sizeY);

        if(pointTouchCanvas(point, canvas.rect))
        {
            point.xMin = Mathf.Max(point.xMin, 0);
            point.yMin = Mathf.Max(point.yMin, 0);
            point.xMax = Mathf.Min(point.xMax, canvas.rect.width);
            point.yMax = Mathf.Min(point.yMax, canvas.rect.height);

            Color[] colors;
            colors = new Color[(int)point.width * (int)point.height];
            for (int j = 0; j < colors.Length; j++)
            {
                colors[j] = color;
            }
            canvas.texture.SetPixels((int)point.x, (int)point.y, (int)point.width, (int)point.height, colors);
        }
    }

    bool pointTouchCanvas(Rect point, Rect canvas)
    {
        float sizeX = point.size.x;
        float sizeY = point.size.y;

        //Debug.Log("x: " + point.x + ", x + size: " + (point.x + sizeX) + ", y:" + point.y + ", y + size: " + (point.y + sizeY) + ", canvas: " + canvas);

        if (point.x + sizeX < 0 || point.y + sizeY < 0 || point.x > canvas.width || point.y > canvas.height)
        {
            return false;
        }

        return true;
    }

    void displayFunctionsToolbar()
    {
        EditorGUILayout.BeginVertical();

        current_color = EditorGUILayout.ColorField(new GUIContent("Color"), current_color, true, true, true);

        //canvas.Texture = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Load texture"), canvas.Texture, typeof(Texture2D), false);

        EditorGUILayout.EndVertical();
    }

    void loadImage()
    {
        string[] extensionFiles = { "Image files", "png,jpg,jpeg", "All files", "*" };
        var selectedImage = EditorUtility.OpenFilePanelWithFilters("Load Image", Application.dataPath, extensionFiles);

        var rawImageData = File.ReadAllBytes(selectedImage);

        Texture2D loadedTexture = new Texture2D(1, 1);
        ImageConversion.LoadImage(loadedTexture, rawImageData);

        canvas.texture = new Texture2D(loadedTexture.width, loadedTexture.height, loadedTexture.format, true);
        Graphics.CopyTexture(loadedTexture, canvas.texture);

        float aspect_ratio = canvas.texture.width / canvas.texture.height;
        float canvas_width = 256f;
        float canvas_height = canvas_width / aspect_ratio;
        canvas.rect = new Rect(300, 200, canvas_width, canvas_height);
    }

    void saveImage()
    {
        var path = EditorUtility.SaveFilePanelInProject("SaveImage", "new_image", "png", "Save Image");

        if (path.Length != 0)
        {
            byte[] bytes = canvas.texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
        }
    }

    void test(object obj)
    {
        Debug.Log(obj);
    }
}
