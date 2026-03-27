using UnityEngine;
using UnityEditor;

namespace UnityEditor.PaintEditor
{
    public class PaintEditorPlugin : EditorSingleton<PaintEditorPlugin>
    {
        public Color currentColor { get; set; }

        public CanvasEditor canvas { get; set; }

        public MainMenu mainMenu { get; set; }

        public Toolbox toolbox { get; set; }

        public CommandHistory history { get; set; }

        public CustomCursor cursor { get; set; }

        [MenuItem("Tools/Raster Editor")]
        public static void CreateEditorWindow()
        {
            GetWindow<EditorWindow>();
            GetWindow(typeof(PaintEditorPlugin));
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            mainMenu = new MainMenu();

            history = new CommandHistory();

            float width, height;
            width = height = 256;
            Rect rect = new Rect(this.position.width / 2 - width / 2, this.position.height / 2 - height / 2, width, height);
            Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, true, false);
            canvas = new CanvasEditor(rect, texture);

            cursor = new CustomCursor(Vector2Int.one);

            currentColor = Color.black;

            toolbox = new Toolbox();

            Repaint();
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            mainMenu.DisplayGUI();

            toolbox.currentTool.Select();

            EditorGUILayout.Space(15);

            EditorGUILayout.BeginHorizontal();

            toolbox.DisplayGUI();

            EditorGUILayout.Space(400);

            canvas.DisplayGUI();

            Event e = Event.current;

            if (e.control && e.keyCode == KeyCode.N && e.type == EventType.KeyDown)
            {
                mainMenu.CreateNewImageWindow();
            }

            if (e.control && e.keyCode == KeyCode.S && e.type == EventType.KeyDown)
            {
                mainMenu.SaveImage();
            }

            if (e.control && e.keyCode == KeyCode.L && e.type == EventType.KeyDown)
            {
                mainMenu.LoadImage();
            }

            if (e.control && e.keyCode == KeyCode.Z)
            {
                ExecuteCommand(new UndoCommand());
            }

            if (e.alt && e.type == EventType.MouseDown)
            {
                toolbox.currentTool = toolbox.pan;
            }
            else if(e.control && e.type == EventType.MouseDown)
            {
                toolbox.currentTool = toolbox.zoom;
            }
            else if (e.type == EventType.MouseUp)
            {
                toolbox.currentTool = toolbox.lastTool;
            }

            if (toolbox.currentTool is Pan)
            {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.Pan);

                if (e.type == EventType.MouseDrag && e.delta != Vector2.zero)
                {
                    canvas.Move(e.delta * toolbox.pan.speed);
                }
            }

            if (e.type == EventType.ScrollWheel)
            {
                EditorGUIUtility.AddCursorRect(new Rect(0,0,this.position.width, this.position.height), MouseCursor.Zoom);
                toolbox.zoom.ChangeZoomLevel(-e.delta.y);
            }
            else if (toolbox.currentTool is Zoom)
            {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.Zoom);

                if (e.type == EventType.MouseDrag && e.delta != Vector2.zero)
                {
                    toolbox.zoom.ChangeZoomLevel(-e.delta.y);
                }
            }

            if (canvas.rect.Contains(e.mousePosition))
            {
                if (toolbox.currentTool is Brush)
                {
                    cursor.Render();
                }
            }

            if ( (e.type == EventType.MouseDown || e.type == EventType.MouseDrag) )
            {
                if (toolbox.currentTool is Brush && toolbox.currentTool is not Eraser)
                {
                    ExecuteCommand(new DrawCommand());
                }
                else if (toolbox.currentTool is Eraser)
                {
                    ExecuteCommand(new EraseCommand());
                }
            }

            displayFunctionsToolbar();

            Repaint();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        void displayOptionsToolbar(ITool currentTool)
        {
            EditorGUILayout.BeginHorizontal();

            currentTool.Select();

            EditorGUILayout.EndHorizontal();
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