using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UnityEditor.PaintEditor
{
    public class PaintEditorPlugin : EditorWindow
    {
        public ITool currentTool { get; private set; }

        public Color currentColor { get; set; }

        public CanvasEditor canvas { get; set; }

        public MainMenuEditor mainMenu { get; set; }

        public CommandHistory history { get; set; }

        //private Texture2D customCursor;

        [MenuItem("Tools/Raster Editor")]
        public static void CreateEditorWindow()
        {
            GetWindow<EditorWindow>();
            GetWindow(typeof(PaintEditorPlugin));
        }

        public void OnEnable()
        {
            currentColor = Color.black;
            currentTool = new Brush(1, 100, Vector2.one, 0);

            float width, height;
            width = height = 256;
            Rect rect = new Rect(this.position.width / 2 - width / 2, this.position.height / 2 - height / 2, width, height);
            Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, true, false);
            canvas = new CanvasEditor(this, rect, texture);

            mainMenu = new MainMenuEditor(this);

            history = new CommandHistory();

            //customCursor = Resources.Load<Texture2D>("cursor.png");
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

            //Repaint();
            //if(Event.current.type == EventType.Repaint)
            //{
            //    Debug.Log("A");
            //    UnityEngine.Cursor.SetCursor(customCursor, new Vector2(16, 16), CursorMode.Auto);
            //    EditorGUIUtility.AddCursorRect(canvas.rect, MouseCursor.CustomCursor);
            //}

            if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
            {
                if (currentTool is Brush && currentTool is not Eraser)
                {
                    ExecuteCommand(new DrawCommand(this));
                }
                else if (currentTool is Eraser)
                {
                    ExecuteCommand(new EraseCommand(this));
                }
            }

            if (Event.current.control && Event.current.keyCode == KeyCode.Z)
            {
                ExecuteCommand(new UndoCommand(this));
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

            EditorGUILayout.EndVertical();
        }

        public void ExecuteCommand(ACommand command)
        {
            if (command.Execute())
            {
                history.Push(command);
            }
        }

        public void Undo()
        {
            ACommand command = history.Pop();
            if (command != null)
            {
                command.Undo();
            }
        }
    }
}