using UnityEngine;

namespace UnityEditor.PaintEditor
{
    public class PaintEditorPlugin : EditorSingleton<PaintEditorPlugin>
    {
        private bool cancelClick = false;

        private const int defaultResolution = 256;
        private const int baseSizeCanvas = 512;
        private Vector2 currentMousePos = Vector2.zero;

        public float angle = 0;

        public CanvasEditor canvas { get; set; }

        public MainMenu mainMenu { get; set; }

        public Toolbox toolbox { get; set; }

        public CommandHistory history { get; set; }

        public CustomCursor cursor { get; set; }

        public Utils utils { get; set; }

        public LayerSelection layerSelection { get; set; }

        [MenuItem("Tools/Paint Editor")]
        public static void CreateEditorWindow()
        {
            GetWindow<EditorWindow>();
            GetWindow(typeof(PaintEditorPlugin));
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            float width, height;
            width = height = defaultResolution;     // defaultResolution is the default realSize
            Rect rect = new Rect(0, 0, width, height);
            float baseZoom = Mathf.FloorToInt(baseSizeCanvas / width);    //baseSizeCanvas is the base size to stretch the canvas rect when opening the editor

            mainMenu = new MainMenu();
            history = new CommandHistory();
            toolbox = new Toolbox(1f, 1f);
            cursor = new CustomCursor(Vector2Int.one);
            canvas = new CanvasEditor(rect);

            SetBaseZoom(baseZoom);

            utils = new Utils();
            utils.currentColor = Color.black;
            layerSelection = new LayerSelection();
        }

        public void OnGUI()
        {
            currentMousePos = Event.current.mousePosition;

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

            if (e.control && e.shift && e.keyCode == KeyCode.Z && e.type == EventType.KeyDown)
            {
                Redo();
            }
            else if (e.control && e.keyCode == KeyCode.Z && e.type == EventType.KeyDown)
            {
                Undo();
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

            if (toolbox.currentTool is Selection)
            {
                if(layerSelection.selectionType == LayerSelection.SelectionType.transform)
                {
                    HandleTransformMode(e);
                }
                else if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && layerSelection.selectionType == LayerSelection.SelectionType.edit)
                {
                    if (layerSelection.rect.Contains(e.mousePosition))
                    {
                        //Move layer handle
                        layerSelection.PixelMove(e.delta);
                    } else
                    {
                        //Click outside the selection merge closes the selection
                        CloseSelection();
                    }
                }
                else if (e.type == EventType.MouseDown && layerSelection.selectionType == LayerSelection.SelectionType.close)
                {
                    //Start selection
                    layerSelection.Open(e.mousePosition, canvas);
                }
                else if(e.type == EventType.MouseDrag && layerSelection.selectionType == LayerSelection.SelectionType.open)
                {
                    //Expand selection
                    layerSelection.Expand(e.mousePosition, canvas.rect);
                }
                else if(e.type == EventType.MouseUp && layerSelection.selectionType == LayerSelection.SelectionType.open)
                {
                    if (layerSelection.IsWidthAndHeightGreaterThanZero())
                    {
                        //Take section of texture to edit
                        layerSelection.Edit();
                        ExecuteCommand(new SelectCommand(layerSelection, canvas.rect, canvas.realSize));
                    }
                    else
                    {
                        //If click and stop clicking in same place, close selection
                        CloseSelection();
                    }
                }
            }

            if (toolbox.currentTool is Pan)
            {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.Pan);

                if (e.type == EventType.MouseDrag && e.delta != Vector2.zero)
                {
                    ExecuteCommand(new PanCommand(e.delta * toolbox.pan.speed, canvas.rect, this.position));
                    Repaint();
                }
            }
            
            if (e.type == EventType.ScrollWheel || (toolbox.currentTool is Zoom))
            {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.Zoom);

                if (layerSelection.selectionType == LayerSelection.SelectionType.edit ||
                    layerSelection.selectionType == LayerSelection.SelectionType.transform)
                {
                    CloseSelection();
                }

                ExecuteCommand(new ZoomCommand(toolbox.zoom, -e.delta.y, e.type));
            }
            
            if (toolbox.currentTool is Brush && toolbox.currentTool is not Eraser)
            {
                cursor.Render();
                Brush brush = (Brush)toolbox.currentTool;

                if(layerSelection.selectionType == LayerSelection.SelectionType.transform)
                {
                    CloseSelection();
                }

                if (layerSelection.selectionType == LayerSelection.SelectionType.edit)
                {
                    DrawCommand command = new DrawCommand(layerSelection.textureSection, layerSelection.rect, layerSelection.rect, layerSelection.realSize, e.mousePosition, utils.currentColor, brush.size, e);
                    ExecuteCommand(command);

                    if(canvas.rect.Contains(e.mousePosition) && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
                        layerSelection.Rotate(0);
                }
                else
                {
                    DrawCommand command = new DrawCommand(canvas.selectedLayer.rTexture, canvas.rect, canvas.rect, canvas.realSize, e.mousePosition, utils.currentColor, brush.size, e);
                    ExecuteCommand(command);
                }
                
            }
            else if (toolbox.currentTool is Eraser)
            {
                cursor.Render();
                Eraser eraser = (Eraser)toolbox.currentTool;

                if (layerSelection.selectionType == LayerSelection.SelectionType.transform)
                {
                    CloseSelection();
                }

                if (layerSelection.selectionType == LayerSelection.SelectionType.edit)
                {
                    EraseCommand command = new EraseCommand(layerSelection.textureSection, layerSelection.rect, layerSelection.rect, layerSelection.realSize, e.mousePosition, eraser.size, e);
                    ExecuteCommand(command);

                    if(canvas.rect.Contains(e.mousePosition) && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
                        layerSelection.Rotate(0);
                }
                else
                {
                    EraseCommand command = new EraseCommand(canvas.selectedLayer.rTexture, canvas.rect, canvas.rect, canvas.realSize, e.mousePosition, eraser.size, e);
                    ExecuteCommand(command);
                }
                
            }
            
            if (toolbox.currentTool is BucketFill)
            {
                if (layerSelection.selectionType == LayerSelection.SelectionType.transform)
                {
                    CloseSelection();
                }

                if (layerSelection.selectionType == LayerSelection.SelectionType.edit )
                {
                    if (layerSelection.rect.Contains(e.mousePosition))
                    {
                        FillCommand command = new FillCommand(layerSelection.textureSection, layerSelection.rect, layerSelection.rect, layerSelection.realSize, e.mousePosition, utils.currentColor, e.type);
                        ExecuteCommand(command);
                        Repaint();

                        if (canvas.rect.Contains(e.mousePosition) && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
                            layerSelection.Rotate(0);
                    }
                }
                else if (canvas.rect.Contains(e.mousePosition))
                {
                    FillCommand command = new FillCommand(canvas.selectedLayer.rTexture, canvas.rect, canvas.rect, canvas.realSize, e.mousePosition, utils.currentColor, e.type);
                    ExecuteCommand(command);
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
                ACommand command = history.RetrievePrevious();

                if(layerSelection.selectionType == LayerSelection.SelectionType.edit ||
                    layerSelection.selectionType == LayerSelection.SelectionType.transform)
                {
                    CloseSelection();
                }

                if (command != null)
                {
                    command.Undo();
                }
            }
        }

        public void Redo()
        {
            if (history.history.Count > 0)
            {
                ACommand command = history.RetrieveNext();

                if (command != null)
                {
                    if (layerSelection.selectionType == LayerSelection.SelectionType.edit ||
                    layerSelection.selectionType == LayerSelection.SelectionType.transform)
                    {
                        CloseSelection();
                    }

                    command.Redo();
                }
            }
        }

        public string ComputePath()
        {
            return "Assets/Editor/PaintEditorExtension/Shaders/";
        }

        private void OnDisable()
        {
            foreach (var layer in canvas.layerList)
            {
                layer.Release();
            }

            canvas.layerList.Clear();
            canvas.layerList = null;
            layerSelection.Clear();

            CommonPaintEditor.Release();
        }

        public float GetBaseZoom()
        {
            return toolbox.zoom.baseZoom;
        }

        public float GetBaseSizeCanvas()
        {
            return baseSizeCanvas;
        }

        public float GetZoomLevel()
        {
            return toolbox.zoom.zoomLevel;
        }

        public void SetBaseZoom(float zoom)
        {
            toolbox.zoom.SetBaseZoom(zoom);
        }

        public Vector2 GetCanvasMousePosition()
        {
            return CommonPaintEditor.ConvertPos(CommonPaintEditor.PosInRect(currentMousePos, canvas.rect), canvas.rect, canvas.realSize);
        }

        public bool IsMouseInCanvas()
        {
            return canvas.rect.Contains(Event.current.mousePosition);
        }

        public void CancelClick(bool value)
        {
            cancelClick = value;
        }

        public void ResetEditor(float baseZoom)
        {
            SetBaseZoom(Mathf.Max(0.5f, Mathf.FloorToInt(baseZoom)));
            layerSelection.Close();
        }

        public void CloseSelection()
        {
            if(layerSelection.textureSection != null)
            {
                ExecuteCommand(new MergeCommand(layerSelection, canvas.rect));
            }
            layerSelection.Close();
            angle = 0;
        }

        public void HandleTransformMode(Event e)
        {
            Vector2 handleUpL = layerSelection.GetHandle(LayerSelection.HandleType.upL);
            Vector2 handleLowL = layerSelection.GetHandle(LayerSelection.HandleType.lowL);
            Vector2 handleUpR = layerSelection.GetHandle(LayerSelection.HandleType.upR);
            Vector2 handleLowR = layerSelection.GetHandle(LayerSelection.HandleType.lowR);

            bool checkUpL = layerSelection.IsPosInHandle(e.mousePosition, handleUpL, 10);
            bool checkLowL = layerSelection.IsPosInHandle(e.mousePosition, handleLowL, 10);
            bool checkUpR = layerSelection.IsPosInHandle(e.mousePosition, handleUpR, 10);
            bool checkLowR = layerSelection.IsPosInHandle(e.mousePosition, handleLowR, 10);

            if (checkUpL)
            {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.ResizeUpLeft);
            }
            else if (checkLowL)
            {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.ResizeUpRight);
            }
            else if (checkUpR)
            {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.ResizeUpRight);
            }
            else if (checkLowR)
            {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.ResizeUpLeft);
            }
            else
            {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.RotateArrow);
            }

            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                if (checkUpL)
                {
                    EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.ResizeUpLeft);
                    layerSelection.Scale(LayerSelection.HandleType.upL, e.delta);
                }
                else if (checkLowL)
                {
                    EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, -this.position.height), MouseCursor.ResizeUpLeft);
                    layerSelection.Scale(LayerSelection.HandleType.lowL, e.delta);
                }
                else if (checkUpR)
                {
                    EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.ResizeUpRight);
                    layerSelection.Scale(LayerSelection.HandleType.upR, e.delta);
                }
                else if(checkLowR)
                {
                    EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, -this.position.height), MouseCursor.ResizeUpRight);
                    layerSelection.Scale(LayerSelection.HandleType.lowR, e.delta);
                }
                else
                {
                    EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, -this.position.height), MouseCursor.RotateArrow);
                    layerSelection.Rotate(e.delta.x * 0.01f);
                }
            }
        }
    }
}