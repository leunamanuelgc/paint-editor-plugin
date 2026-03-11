using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class RasterGraphicsEditorWindow : EditorWindow
{
    enum ToolType
    {
        select,
        brush,
        eraser,
    }

    ToolType current_tool;

    public Texture2D texture;
    string texture_name;

    float aspect_ratio, canvas_width, canvas_height;
    Rect rect_canvas;

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
    public static void CreateExample()
    {
        EditorWindow.GetWindow<EditorWindow>();
        EditorWindow.GetWindow(typeof(RasterGraphicsEditorWindow));
    }

    public void OnEnable()
    {
        aspect_ratio = 1f;
        canvas_width = 256f;
        canvas_height = canvas_width * aspect_ratio;
        rect_canvas = new Rect(300, 200, canvas_width, canvas_height);

        texture = new Texture2D((int)Mathf.Ceil(canvas_width), (int)Mathf.Ceil(canvas_height));
        texture_name = texture.name;

        current_color = Color.black;

        min_brush_size = 1;
        max_brush_size = 100;
        brush_vector_size = new Vector2Int(1, 1);
        brush_type_index = 0;

        current_tool = ToolType.brush;
    }

    public void OnGUI()
    {
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

    bool isMouseInsideRect(Rect rect_to_check)
    {
        return (Event.current.mousePosition.x > rect_to_check.x && Event.current.mousePosition.x < rect_to_check.x + rect_to_check.width)
            && (Event.current.mousePosition.y > rect_to_check.y && Event.current.mousePosition.y < rect_to_check.y + rect_to_check.height);
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
        if (EditorGUILayout.DropdownButton(new GUIContent("Option 1"), FocusType.Keyboard, EditorStyles.toolbarButton))
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Test/1"), true, test, 1);

            menu.ShowAsContext();
        }
    }

    void displayToolboxToolbar()
    {
        EditorGUILayout.BeginVertical();

        if (EditorGUILayout.DropdownButton(new GUIContent("select"), FocusType.Keyboard, EditorStyles.toolbarButton))
        {
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
        //EditorGUILayout.RectField(rect_canvas);

        EditorGUI.DrawPreviewTexture(rect_canvas, texture, null, ScaleMode.ScaleToFit, (float)texture.width / (float)texture.height);

        if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
        {
            if (isMouseInsideRect(rect_canvas))
            {
                float x_mouse_pos_in_canvas = Event.current.mousePosition.x - rect_canvas.x;
                float y_mouse_pos_in_canvas = rect_canvas.height - (Event.current.mousePosition.y - rect_canvas.y);

                float x_canvas_convertion = (float)texture.width / (float)canvas_width;
                float y_canvas_convertion = (float)texture.height / (float)canvas_height;

                float x_pos_in_texture;
                float y_pos_in_texture;

                x_pos_in_texture = x_mouse_pos_in_canvas * x_canvas_convertion;
                y_pos_in_texture = y_mouse_pos_in_canvas * y_canvas_convertion;

                Color[] colors = new Color[brush_vector_size.x * brush_vector_size.y];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = current_color;
                }

                texture.SetPixels((int)x_pos_in_texture, (int)y_pos_in_texture, brush_vector_size.x, brush_vector_size.y, colors);
                texture.Apply();
                Repaint();
            }
        }
    }

    void displayFunctionsToolbar()
    {
        EditorGUILayout.BeginVertical();

        current_color = EditorGUILayout.ColorField(new GUIContent("Color"), current_color, true, true, true);

        //texture = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Load texture"), texture, typeof(Texture2D), false);

        EditorGUILayout.EndVertical();
    }

    void loadImage()
    {
        string[] extensionFiles = { "Image files", "png,jpg,jpeg", "All files", "*" };
        var selectedImage = EditorUtility.OpenFilePanelWithFilters("Load Image", Application.dataPath, extensionFiles);

        var rawImageData = File.ReadAllBytes(selectedImage);

        Texture2D loadedTexture = new Texture2D(1, 1);
        ImageConversion.LoadImage(loadedTexture, rawImageData);

        texture = new Texture2D(loadedTexture.width, loadedTexture.height, loadedTexture.format, true);
        Graphics.CopyTexture(loadedTexture, texture);

        aspect_ratio = (float)texture.width / (float)texture.height;
        canvas_width = 256f;
        canvas_height = canvas_width / aspect_ratio;
        rect_canvas.width = canvas_width;
        rect_canvas.height = canvas_height;
    }

    void saveImage()
    {
        var path = EditorUtility.SaveFilePanelInProject("SaveImage", "new_image", "png", "Save Image");

        if (path.Length != 0)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
        }
    }

    void test(object obj)
    {
        Debug.Log(obj);
    }
}
