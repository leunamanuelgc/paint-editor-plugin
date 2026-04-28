using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class PaintEditorPlugin : EditorSingleton<PaintEditorPlugin>
    {
        public CanvasEditor canvas { get; set; }

        public MainMenu mainMenu { get; set; }

        public Toolbox toolbox { get; set; }

        public CommandHistory history { get; set; }

        public CustomCursor cursor { get; set; }

        public Utils utils { get; set; }

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

            toolbox = new Toolbox();

            cursor = new CustomCursor(Vector2Int.one);

            float width, height;
            width = height = 256;
            Rect rect = new Rect(0, 0, width, height);
            canvas = new CanvasEditor(rect);

            utils = new Utils();
            utils.currentColor = Color.black;
        }

        public void OnGUI()
        {
            canvas.DisplayGUI();

            EditorGUILayout.BeginVertical();

            mainMenu.DisplayGUI();

            EditorGUILayout.EndVertical();

            toolbox.currentTool.Select();

            BeginWindows();

            toolbox.DisplayGUI();

            utils.DisplayGUI();

            EndWindows();

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

            if (e.control && e.keyCode == KeyCode.Z && e.type == EventType.KeyDown)
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
                    Repaint();
                }
            }

            if (e.type == EventType.ScrollWheel)
            {
                EditorGUIUtility.AddCursorRect(new Rect(0,0,this.position.width, this.position.height), MouseCursor.Zoom);
                SetZoom(-e.delta.y);
            }
            else if (toolbox.currentTool is Zoom)
            {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.Zoom);

                if (e.type == EventType.MouseDrag && e.delta != Vector2.zero)
                {
                    SetZoom(-e.delta.y);
                }
            }

            if (canvas.rect.Contains(e.mousePosition))
            {
                if (toolbox.currentTool is Brush)
                {
                    cursor.Render();
                }
            }

            if (e.type == EventType.MouseDown)
            {
                if (toolbox.currentTool is Brush && toolbox.currentTool is not Eraser)
                {
                    Brush brush = (Brush)toolbox.currentTool;
                    DrawCommand command = new DrawCommand(canvas.selectedLayer, canvas.rect, canvas.realSize, e.mousePosition, utils.currentColor, brush.size);
                    command.SaveBackup();
                    history.Push(command);
                    command.Execute();
                }
                else if (toolbox.currentTool is Eraser)
                {
                    Eraser eraser = (Eraser)toolbox.currentTool;
                    EraseCommand command = new EraseCommand(canvas.selectedLayer, canvas.rect, canvas.realSize, e.mousePosition, eraser.size);
                    command.SaveBackup();
                    history.Push(command);
                    command.Execute();
                }
                else if(toolbox.currentTool is BucketFill)
                {
                    if (canvas.rect.Contains(e.mousePosition))
                    {
                        FillCommand command = new FillCommand(canvas.selectedLayer, canvas.rect, canvas.realSize, e.mousePosition, utils.currentColor);
                        command.SaveBackup();
                        history.Push(command);
                        command.Execute();
                        Repaint();
                    }
                }
            }

            if (e.type == EventType.MouseDrag)
            {
                if (toolbox.currentTool is Brush && toolbox.currentTool is not Eraser)
                {
                    Brush brush = (Brush)toolbox.currentTool;
                    DrawCommand command = new DrawCommand(canvas.selectedLayer, canvas.rect, canvas.realSize, e.mousePosition, utils.currentColor, brush.size);
                    command.Execute();
                }
                else if (toolbox.currentTool is Eraser)
                {
                    Eraser eraser = (Eraser)toolbox.currentTool;
                    EraseCommand command = new EraseCommand(canvas.selectedLayer, canvas.rect, canvas.realSize, e.mousePosition, eraser.size);
                    command.Execute();
                }
            }
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
            if (history.history.Count > 0)
            {
                ACommand command = history.Pop();

                if (command != null)
                {
                    command.Undo();
                }
            }
        }

        public string ComputePath()
        {
            return "Assets/Editor/PaintEditorPlugin/ComputeShaders/";
        }

        private void OnDisable()
        {
            foreach (var layer in canvas.layerList)
            {
                layer.Release();
            }

            canvas.layerList.Clear();
            canvas.layerList = null;
        }

        public float GetZoomLevel()
        {
            return toolbox.zoom.zoomLevel;
        }

        public void SetZoom(float zoom)
        {
            toolbox.zoom.ChangeZoomLevel(zoom);
        }
    }
}