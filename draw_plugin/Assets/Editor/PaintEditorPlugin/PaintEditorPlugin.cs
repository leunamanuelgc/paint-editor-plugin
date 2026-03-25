using UnityEngine;
using UnityEditor;

namespace UnityEditor.PaintEditor
{
    public class PaintEditorPlugin : EditorSingleton<PaintEditorPlugin>
    {
        private Brush brush;

        private Eraser eraser;

        private Pan pan;

        private Zoom zoom;

        public ITool currentTool { get; private set; }
        public ITool lastTool { get; private set; }

        public Color currentColor { get; set; }

        public CanvasEditor canvas { get; set; }

        public MainMenuEditor mainMenu { get; set; }

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

            mainMenu = new MainMenuEditor(this);

            history = new CommandHistory();

            float width, height;
            width = height = 256;
            Rect rect = new Rect(this.position.width / 2 - width / 2, this.position.height / 2 - height / 2, width, height);
            Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, true, false);
            canvas = new CanvasEditor(this, rect, texture);

            cursor = new CustomCursor(this, Vector2Int.one);

            currentColor = Color.black;

            brush = new Brush(1, 100, Vector2.one, 0);
            eraser = new Eraser(1, 100, Vector2.one, 0);
            pan = new Pan(1f);
            zoom = new Zoom(1f);

            currentTool = brush;
            lastTool = currentTool;

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
                ExecuteCommand(new UndoCommand(this));
            }

            if (e.alt && e.type == EventType.MouseDown)
            {
                currentTool = pan;
            }
            else if (e.type == EventType.MouseUp)
            {
                currentTool = lastTool;
            }

            if (canvas.rect.Contains(e.mousePosition))
            {
                if (currentTool is Brush)
                {
                    cursor.Render();
                }
            }

            if ( (e.type == EventType.MouseDown || e.type == EventType.MouseDrag) )
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

            if (currentTool is Pan && GUIUtility.hotControl == 0)
            {
                EditorGUIUtility.AddCursorRect(this.position, MouseCursor.Pan);

                if (e.type == EventType.MouseDrag && e.delta != Vector2.zero)
                {
                    canvas.Move(e.delta * pan.speed);
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

        void displayToolboxToolbar()
        {
            EditorGUILayout.BeginVertical();

            if (EditorGUILayout.DropdownButton(new GUIContent("Brush"), FocusType.Keyboard, EditorStyles.toolbarButton))
            {
                currentTool = brush;
                lastTool = currentTool;
                currentTool.Select();
            }

            if (EditorGUILayout.DropdownButton(new GUIContent("Eraser"), FocusType.Keyboard, EditorStyles.toolbarButton))
            {
                currentTool = eraser;
                lastTool = currentTool;
                currentTool.Select();
            }

            if (EditorGUILayout.DropdownButton(new GUIContent("Pan"), FocusType.Keyboard, EditorStyles.toolbarButton))
            {
                currentTool = pan;
                lastTool = currentTool;
                currentTool.Select();
            }

            if (EditorGUILayout.DropdownButton(new GUIContent("Zoom"), FocusType.Keyboard, EditorStyles.toolbarButton))
            {
                currentTool = zoom;
                lastTool = currentTool;
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