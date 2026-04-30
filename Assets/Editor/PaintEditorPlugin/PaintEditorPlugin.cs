using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class PaintEditorPlugin : EditorSingleton<PaintEditorPlugin>
    {
        private bool cancelClick = false;

        public CanvasEditor canvas { get; set; }

        public MainMenu mainMenu { get; set; }

        public Toolbox toolbox { get; set; }

        public CommandHistory history { get; set; }

        public CustomCursor cursor { get; set; }

        public Utils utils { get; set; }

        public LayerSelection layerSelection { get; set; }

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
            DisplayGUI();

            if (!cancelClick)
            {
                HandleShortcuts();

                HandleTools();
            }
            else
            {
                if (Event.current.type == EventType.MouseUp)
                {
                    cancelClick = false;
                }
            }
        }

        public void DisplayGUI()
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

            if(layerSelection != null)
            {
                layerSelection.DisplayGUI();
                Repaint();
            }
        }

        public void HandleShortcuts()
        {
            Event e = Event.current;

            if (e.control && e.keyCode == KeyCode.N && e.type == EventType.KeyDown)
            {
                mainMenu.CreateNewImageWindow();
            }

            if (e.control && e.keyCode == KeyCode.S && e.type == EventType.KeyDown)
            {
                ExecuteCommand(new SaveCommand());
            }

            if (e.control && e.keyCode == KeyCode.L && e.type == EventType.KeyDown)
            {
                ExecuteCommand(new LoadCommand());
            }

            if (e.control && e.keyCode == KeyCode.Z && e.type == EventType.KeyDown)
            {
                ExecuteCommand(new UndoCommand());
            }

            if (e.alt && e.type == EventType.MouseDown)
            {
                toolbox.currentTool = toolbox.pan;
            }
            else if (e.control && e.type == EventType.MouseDown)
            {
                toolbox.currentTool = toolbox.zoom;
            }
            else if (e.type == EventType.MouseUp)
            {
                toolbox.currentTool = toolbox.lastTool;
            }
        }

        public void HandleTools()
        {
            Event e = Event.current;

            if(toolbox.currentTool is Selection)
            {
                if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) &&
                    layerSelection != null && layerSelection.rect.Contains(e.mousePosition) && layerSelection.selectionType == LayerSelection.SelectionType.edit)
                {
                    layerSelection.Move(e.delta);
                }
                else if(e.type == EventType.MouseDown && layerSelection != null && layerSelection.selectionType == LayerSelection.SelectionType.edit)
                {
                    ExecuteCommand(new MergeCommand(layerSelection, canvas.rect));
                    layerSelection.Clear();
                    layerSelection = null;
                }
                else if (e.type == EventType.MouseDown && layerSelection == null)
                {
                    layerSelection = new LayerSelection(e.mousePosition, canvas);
                }
                else if(e.type == EventType.MouseDrag && layerSelection != null && layerSelection.selectionType == LayerSelection.SelectionType.open)
                {
                    layerSelection.Expand(e.mousePosition, canvas.rect);
                }
                else if(e.type == EventType.MouseUp && layerSelection != null && layerSelection.selectionType == LayerSelection.SelectionType.open)
                {
                    if (layerSelection.IsWidthAndHeightGreaterThanZero())
                    {
                        ExecuteCommand(new SelectCommand(layerSelection, canvas.rect, canvas.realSize));
                    }
                    else
                    {
                        layerSelection.Clear();
                        layerSelection = null;
                    }
                }
            }
            else if (toolbox.currentTool is Pan)
            {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.Pan);

                if (e.type == EventType.MouseDrag && e.delta != Vector2.zero)
                {
                    ExecuteCommand(new PanCommand(e.delta * toolbox.pan.speed, canvas.rect, this.position));
                    Repaint();
                }
            }
            else if (e.type == EventType.ScrollWheel || toolbox.currentTool is Zoom)
            {
                if (layerSelection != null)
                {
                    ExecuteCommand(new MergeCommand(layerSelection, canvas.rect));
                    layerSelection.Clear();
                    layerSelection = null;
                }
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.Zoom);
                ExecuteCommand(new ZoomCommand(toolbox.zoom, -e.delta.y));
            }
            else if (toolbox.currentTool is Brush && toolbox.currentTool is not Eraser)
            {
                cursor.Render();
                Brush brush = (Brush)toolbox.currentTool;

                if (layerSelection != null && layerSelection.selectionType == LayerSelection.SelectionType.edit)
                {
                    DrawCommand command = new DrawCommand(layerSelection.textureSection, layerSelection.textureRect, layerSelection.rect, canvas.realSize, e.mousePosition, utils.currentColor, brush.size, e.type);
                    ExecuteCommand(command);
                }
                else
                {
                    DrawCommand command = new DrawCommand(canvas.selectedLayer.rTexture, canvas.rect, canvas.rect, canvas.realSize, e.mousePosition, utils.currentColor, brush.size, e.type);
                    ExecuteCommand(command);
                }
                
            }
            else if (toolbox.currentTool is Eraser)
            {
                cursor.Render();
                Eraser eraser = (Eraser)toolbox.currentTool;
                if (layerSelection != null && layerSelection.selectionType == LayerSelection.SelectionType.edit)
                {
                    EraseCommand command = new EraseCommand(layerSelection.textureSection, layerSelection.textureRect, layerSelection.rect, canvas.realSize, e.mousePosition, eraser.size, e.type);
                    ExecuteCommand(command);
                }
                else
                {
                    EraseCommand command = new EraseCommand(canvas.selectedLayer.rTexture, canvas.rect, canvas.rect, canvas.realSize, e.mousePosition, eraser.size, e.type);
                    ExecuteCommand(command);
                }
                
            }
            else if (toolbox.currentTool is BucketFill)
            {
                if (canvas.rect.Contains(e.mousePosition))
                {
                    FillCommand command = new FillCommand(canvas.selectedLayer, canvas.rect, canvas.realSize, e.mousePosition, utils.currentColor, e.type);
                    command.SaveBackup();
                    history.Push(command);
                    command.Execute();
                    Repaint();
                }
            }

            if (e.type == EventType.MouseUp)
            {
                Repaint();
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
            toolbox.zoom.SetZoomLevel(zoom);
        }

        public bool IsMouseInCanvas()
        {
            return canvas.rect.Contains(Event.current.mousePosition);
        }

        public void CancelClick(bool value)
        {
            cancelClick = value;
        }
    }
}