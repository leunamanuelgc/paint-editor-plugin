using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;

namespace UnityEditor.PaintEditor
{
    public class PaintEditorPlugin : EditorWindow
    {
        public ITool currentTool { get; private set; }

        public Color currentColor { get; set; }

        public CanvasEditor canvas { get; set; }
        public MainMenuEditor mainMenu { get; set; }

        [MenuItem("Tools/Raster Editor")]
        public static void CreateEditorWindow()
        {
            GetWindow<EditorWindow>();
            GetWindow(typeof(PaintEditorPlugin));
        }

        public void OnEnable()
        {
            float width, height;
            width = height = 256;
            Rect rect = new Rect(this.position.width / 2 - width / 2, this.position.height / 2 - height / 2, width, height);
            Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, true, false);
            canvas = new CanvasEditor(this, rect, texture);

            mainMenu = new MainMenuEditor(this);

            currentColor = Color.black;
            currentTool = new Brush(1, 100, Vector2.one, 0);

            Repaint();
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            mainMenu.DisplayGUI();

            displayOptionsToolbar(currentTool);

            EditorGUILayout.Space(15);

            EditorGUILayout.BeginHorizontal();

            displayToolboxToolbar();

            EditorGUILayout.Space(400);

            canvas.DisplayGUI();

            if (Event.current.control && Event.current.keyCode == KeyCode.S)
            {
                mainMenu.SaveImage();
            }

            if (Event.current.control && Event.current.keyCode == KeyCode.L)
            {
                mainMenu.LoadImage();
            }

            if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
            {
                if (currentTool is Brush)
                {
                    currentTool.SetCommand(new DrawCommand(this));
                    currentTool.Use();
                }
            }

            displayFunctionsToolbar();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        void displayOptionsToolbar(ITool currentTool)
        {
            EditorGUILayout.BeginHorizontal();

            currentTool.Select();

            EditorGUILayout.EndHorizontal();
        }

        void displayToolboxToolbar()
        {
            EditorGUILayout.BeginVertical();

            if (EditorGUILayout.DropdownButton(new GUIContent("Brush"), FocusType.Keyboard, EditorStyles.toolbarButton))
            {
                currentTool = new Brush(1, 100, Vector2.one, 0);
                currentTool.Select();
            }

            if (EditorGUILayout.DropdownButton(new GUIContent("Eraser"), FocusType.Keyboard, EditorStyles.toolbarButton))
            {
                currentTool = new Eraser(1, 100, Vector2.one, 0);
                currentTool.Select();
            }

            EditorGUILayout.EndVertical();
        }

        void displayFunctionsToolbar()
        {
            EditorGUILayout.BeginVertical();

            currentColor = EditorGUILayout.ColorField(new GUIContent("Color"), currentColor, true, true, true);

            //canvas.Texture = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Load texture"), canvas.Texture, typeof(Texture2D), false);

            EditorGUILayout.EndVertical();
        }
    }
}