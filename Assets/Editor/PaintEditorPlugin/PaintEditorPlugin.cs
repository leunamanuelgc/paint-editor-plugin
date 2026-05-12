using System.Data;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;

namespace UnityEditor.PaintEditor
{
    public class PaintEditorPlugin : EditorSingleton<PaintEditorPlugin>
    {
        private bool cancelClick = false;

        private const int defaultResolution = 256;
        private const int baseSizeCanvas = 512;

        public float angle = 0;

        public CanvasEditor canvas { get; set; }

        public MainMenu mainMenu { get; set; }

        public Toolbox toolbox { get; set; }

        public CommandHistory history { get; set; }

        public CustomCursor cursor { get; set; }

        public Utils utils { get; set; }

        public LayerSelection layerSelection { get; set; }

        CustomRenderTexture rT;
        Material m;

        [MenuItem("Tools/Raster Editor")]
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
            float baseZoom = baseSizeCanvas / width;    //baseSizeCanvas is the base size to stretch the canvas rect when opening the editor

            mainMenu = new MainMenu();
            history = new CommandHistory();
            toolbox = new Toolbox(1f, 1f);
            cursor = new CustomCursor(Vector2Int.one);
            canvas = new CanvasEditor(rect);

            SetBaseZoom(baseZoom);

            utils = new Utils();
            utils.currentColor = Color.black;
            layerSelection = new LayerSelection();


            //////////////

            //rT = new CustomRenderTexture(256, 256, RenderTextureFormat.ARGB32);
            //string path = "Assets/Resources/human-placeholder.png";
            
            //var rawImageData = File.ReadAllBytes(path);

            //Texture2D loadedTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            //loadedTexture.alphaIsTransparency = true;
            //ImageConversion.LoadImage(loadedTexture, rawImageData);

            //rT = new CustomRenderTexture(256, 256, RenderTextureFormat.ARGB32);
            //rT.filterMode = FilterMode.Point;
            //rT.updateMode = CustomRenderTextureUpdateMode.Realtime;
            //rT.enableRandomWrite = true;

            //Shader rotateShader;
            //string rotateShaderPath = ComputePath() + "Mat/Sh/RotateTextures.shader";
            //string shaderName = "Basics/RotateTextures";

            ////rotateShader = AssetDatabase.LoadAssetAtPath<Shader>(rotateShaderPath);
            //rotateShader = Shader.Find(shaderName);

            //m = new Material(rotateShader);
            //Graphics.Blit(loadedTexture, rT);

            //m.SetTexture("_MainTexture", rT);

            //Graphics.Blit(rT, m);

            //rT.material = m;

            //////////////
        }

        public void OnGUI()
        {
            //if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
            //{
            //    CustomRenderTexture newRT = new CustomRenderTexture(256, 256, RenderTextureFormat.ARGB32);

            //    Graphics.Blit(rT, newRT, m);

            //    Graphics.Blit(newRT, rT);
            //}

            //GUI.DrawTexture(new Rect(0, 0, 256, 256), rT);

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

            if (toolbox.currentTool is Selection)
            {
                Vector2 handleUpL = layerSelection.GetHandle(LayerSelection.HandleType.upL);
                Vector2 handleLowL = layerSelection.GetHandle(LayerSelection.HandleType.lowL);
                Vector2 handleUpR = layerSelection.GetHandle(LayerSelection.HandleType.upR);
                Vector2 handleLowR = layerSelection.GetHandle(LayerSelection.HandleType.lowR);
                Vector2 handleRotate = layerSelection.GetHandle(LayerSelection.HandleType.rotate);

                bool checkUpL = layerSelection.IsPosInHandle(e.mousePosition, handleUpL, 10);
                bool checkLowL = layerSelection.IsPosInHandle(e.mousePosition, handleLowL, 10);
                bool checkUpR = layerSelection.IsPosInHandle(e.mousePosition, handleUpR, 10);
                bool checkLowR = layerSelection.IsPosInHandle(e.mousePosition, handleLowR, 10);
                bool checkRotate = layerSelection.IsPosInHandle(e.mousePosition, handleRotate, 100);

                if (layerSelection.selectionType == LayerSelection.SelectionType.edit)
                {   
                    if (checkUpL)
                    {
                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.ResizeUpLeft);
                    }

                    if (checkLowL)
                    {
                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.ResizeUpRight);
                    }

                    if (checkUpR)
                    {
                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.ResizeUpRight);
                    }

                    if (checkLowR)
                    {
                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.ResizeUpLeft);
                    }

                    if (checkRotate)
                    {
                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.RotateArrow);
                    }
                }

                if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && layerSelection.selectionType == LayerSelection.SelectionType.edit)
                {
                    bool mouseInScaleHandlesStatement = checkUpL || checkLowL || checkUpR || checkLowR || checkRotate;

                    if (mouseInScaleHandlesStatement)
                    {
                        //Scale handle
                        if (checkUpL)
                        {
                            EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.ResizeUpLeft);
                            layerSelection.Scale(LayerSelection.HandleType.upL, e.delta);
                        }
                        
                        if (checkLowL)
                        {
                            EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, -this.position.height), MouseCursor.ResizeUpLeft);
                            layerSelection.Scale(LayerSelection.HandleType.lowL, e.delta);
                        }
                        
                        if (checkUpR)
                        {
                            EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.ResizeUpRight);
                            layerSelection.Scale(LayerSelection.HandleType.upR, e.delta);
                        }
                        
                        if (checkLowR)
                        {
                            EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, -this.position.height), MouseCursor.ResizeUpRight);
                            layerSelection.Scale(LayerSelection.HandleType.lowR, e.delta);
                        }

                        if (checkRotate)
                        {
                            EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, -this.position.height), MouseCursor.RotateArrow);
                            layerSelection.Rotate(e.delta.x * 0.01f);

                            //Vector3 direction = e.mousePosition - layerSelection.GetCenter();
                            //Vector3 rotation = Quaternion.LookRotation(direction).eulerAngles;
                            //layerSelection.Rotate(rotation.x);
                            //rotation.x -= 90;
                        
                            //if (rotation.y == 90)
                            //{
                            //    if (Mathf.Abs(rotation.x) <= 90 && rotation.x < 0){
                            //        rotation.x = -rotation.x;
                            //    } else if (Mathf.Abs(rotation.x) <= 270 && rotation.x > 0)
                            //    {
                            //        rotation.x = -rotation.x;
                            //    }
                                
                            //}
                        
                            //Debug.Log(rotation);
                            //angle = rotation.x;
                        }
                    }
                    else if (layerSelection.rect.Contains(e.mousePosition))
                    {
                        //Move layer handle
                        layerSelection.Move(e.delta);
                    } else
                    {
                        //Click outside the selection merge closes the selection
                        ExecuteCommand(new MergeCommand(layerSelection, canvas.rect));
                        layerSelection.Close();
                        angle = 0;
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
                        layerSelection.Close();
                        angle = 0;
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
            
            if (e.type == EventType.ScrollWheel || toolbox.currentTool is Zoom)
            {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, this.position.width, this.position.height), MouseCursor.Zoom);

                if (layerSelection.selectionType == LayerSelection.SelectionType.edit)
                {
                    ExecuteCommand(new MergeCommand(layerSelection, canvas.rect));
                    layerSelection.Close();
                }

                ExecuteCommand(new ZoomCommand(toolbox.zoom, -e.delta.y));
            }
            
            if (toolbox.currentTool is Brush && toolbox.currentTool is not Eraser)
            {
                cursor.Render();
                Brush brush = (Brush)toolbox.currentTool;

                if (layerSelection.selectionType == LayerSelection.SelectionType.edit)
                {
                    DrawCommand command = new DrawCommand(layerSelection.textureSection, layerSelection.rect, layerSelection.rect, layerSelection.realSize, e.mousePosition, utils.currentColor, brush.size, e.type);
                    ExecuteCommand(command);

                    if(canvas.rect.Contains(e.mousePosition) && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
                        layerSelection.Rotate(0);
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
                if (layerSelection.selectionType == LayerSelection.SelectionType.edit)
                {
                    EraseCommand command = new EraseCommand(layerSelection.textureSection, layerSelection.rect, layerSelection.rect, layerSelection.realSize, e.mousePosition, eraser.size, e.type);
                    ExecuteCommand(command);

                    if(canvas.rect.Contains(e.mousePosition) && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
                        layerSelection.Rotate(0);
                }
                else
                {
                    EraseCommand command = new EraseCommand(canvas.selectedLayer.rTexture, canvas.rect, canvas.rect, canvas.realSize, e.mousePosition, eraser.size, e.type);
                    ExecuteCommand(command);
                }
                
            }
            
            if (toolbox.currentTool is BucketFill)
            {
                if (layerSelection.selectionType == LayerSelection.SelectionType.edit )
                {
                    if (layerSelection.rect.Contains(e.mousePosition))
                    {
                        FillCommand command = new FillCommand(layerSelection.textureSection, layerSelection.rect, layerSelection.rect, layerSelection.realSize, e.mousePosition, utils.currentColor, e.type);
                        command.Execute();
                        Repaint();

                        if (canvas.rect.Contains(e.mousePosition) && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
                            layerSelection.Rotate(0);
                    }
                }
                else if (canvas.rect.Contains(e.mousePosition))
                {
                    FillCommand command = new FillCommand(canvas.selectedLayer.rTexture, canvas.rect, canvas.rect, canvas.realSize, e.mousePosition, utils.currentColor, e.type);
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
            return "Assets/Editor/PaintEditorPlugin/Shaders/";
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
            SetBaseZoom(baseZoom);
            layerSelection.Close();
            CancelClick(false);
        }
    }
}