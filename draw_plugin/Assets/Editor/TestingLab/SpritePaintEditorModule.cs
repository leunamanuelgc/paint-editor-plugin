using UnityEditor.Sprites;
using UnityEditor.TerrainTools;
using UnityEditor.U2D.Sprites;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace UnityEditor._2D.SpritePaint.Editor
{
    public class SpritePaintEditorModule : SpriteEditorModuleBase
    {
        private static class Styles
        {
            public static readonly GUIContent panelTitle = EditorGUIUtility.TrTextContent("Sprite Paint");
        }

        Vector2 m_ReorderableListScrollPosition;
        private IMGUIContainer m_PaintToolboxInspectorContainer;

        public override string moduleName
        {
            get { return Styles.panelTitle.text; }
        }

        public override bool ApplyRevert(bool apply)
        {
            if (apply)
            {

            }

            return true;
        }

        public override bool CanBeActivated()
        {
            return true;
        }

        public override void DoPostGUI()
        {

        }

        public void PaintToolboxUI()
        {
            using (new EditorGUI.DisabledScope(spriteEditor.editingDisabled))
            {
                var windowDimension = spriteEditor.windowDimension;

                GUILayout.BeginArea(new Rect(0, 0, 100, 500), Styles.panelTitle, GUI.skin.window);
                m_ReorderableListScrollPosition = GUILayout.BeginScrollView(m_ReorderableListScrollPosition);
                GUILayout.EndScrollView();
                GUILayout.EndArea();

                // Deselect the list item if left click outside the panel area.
                Event e = Event.current;
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    OnSelectCallback();
                    spriteEditor.RequestRepaint();
                }
            }
        }

        public override void DoMainGUI()
        {
            ITextureDataProvider textureDataProvider = spriteEditor.GetDataProvider<ITextureDataProvider>();

            ISpriteEditorDataProvider dataProvider = spriteEditor.GetDataProvider<ISpriteEditorDataProvider>();

            SpriteRect[] spriteRects = dataProvider.GetSpriteRects();
            Vector4 border = spriteRects[0].border;
            SpriteAlignment spriteAlignment = spriteRects[0].alignment;

            Rect canvas = new Rect(spriteEditor.scrollPosition, spriteRects[0].rect.size);

            Debug.Log(spriteEditor.scrollPosition);

            spriteEditor.enableMouseMoveEvent = true;
            Vector2 mousePos = Event.current.mousePosition;

            //Debug.Log("Border " + border.ToString());
            //Debug.Log("SpriteAlignment " + spriteAlignment.ToString());
            //Debug.Log("ScrollPosition " + spriteEditor.scrollPosition);
            //Debug.Log("WindowDimension" + spriteEditor.windowDimension);
            //Debug.Log("Zoom Level" + spriteEditor.zoomLevel);
            //Debug.Log("MousePos " + mousePos);

            


            //VisualElement mainVisualContainer = spriteEditor.GetMainVisualContainer();

            //Debug.Log(mainVisualContainer.childCount);

            //Rect canvas = mainVisualContainer.contentRect;

            

            //Debug.Log(mousePos);

            if (canvas.Contains(mousePos))
            {
                Debug.Log("Inside");
            }

            if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
            {
                if (isMouseInsideRect(canvas))
                {

                    Texture2D original_texture = textureDataProvider.texture;
                    Texture2D texture2D = new Texture2D(original_texture.width, original_texture.height, original_texture.format, original_texture.mipmapCount, true);

                    Graphics.CopyTexture(original_texture, texture2D);

                    spriteEditor.SetPreviewTexture(texture2D, texture2D.width, texture2D.height);

                    float x_mouse_pos_in_canvas = Event.current.mousePosition.x - canvas.x;
                    float y_mouse_pos_in_canvas = canvas.height - (Event.current.mousePosition.y - canvas.y);

                    float x_canvas_convertion = (float)texture2D.width / (float)canvas.width;
                    float y_canvas_convertion = (float)texture2D.height / (float)canvas.height;

                    float x_pos_in_texture;
                    float y_pos_in_texture;

                    x_pos_in_texture = x_mouse_pos_in_canvas * x_canvas_convertion;
                    y_pos_in_texture = y_mouse_pos_in_canvas * y_canvas_convertion;

                    int x = 2, y = 2;
                    Color[] colors = new Color[x * y];
                    for (int i = 0; i < colors.Length; i++)
                    {
                        colors[i] = Color.red;
                    }
                    texture2D.SetPixels((int)x_pos_in_texture, (int)y_pos_in_texture, x, y, colors);

                    texture2D.Apply();
                    spriteEditor.RequestRepaint();

                    spriteEditor.SetDataModified();
                }

            }
        }

        bool isMouseInsideRect(Rect rect_to_check)
        {
            return (Event.current.mousePosition.x > rect_to_check.x && Event.current.mousePosition.x < rect_to_check.x + rect_to_check.width)
                && (Event.current.mousePosition.y > rect_to_check.y && Event.current.mousePosition.y < rect_to_check.y + rect_to_check.height);
        }

        public override void DoToolbarGUI(Rect drawArea)
        {

        }

        public override void OnModuleActivate()
        {
            //m_PaintToolboxInspectorContainer = new IMGUIContainer(PaintToolboxUI)
            //{
            //    style =
            //    {
            //        flexGrow = 0,
            //        flexBasis = 1,
            //        flexShrink = 0,
            //        minWidth = 100,
            //        minHeight = 500,
            //        bottom = 0,
            //        right = 0,
            //        position = new StyleEnum<Position>(Position.Relative)
            //    },
            //    name = "Sprite Paint"
            //};
            //spriteEditor.GetMainVisualContainer().Add(m_PaintToolboxInspectorContainer);
        }

        void OnSelectCallback()
        {

        }

        public override void OnModuleDeactivate()
        {

        }
    }
}

