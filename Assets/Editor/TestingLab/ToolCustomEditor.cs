using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing;
using System.IO;

public class ToolCustomEditor : EditorWindow
{
    private VisualElement m_rightPane;

    private Thread thread;

    System.Drawing.Graphics g;

    [DllImport("Dll2", EntryPoint = "DrawWindow")]
    public static extern void DrawWindow();

    [UnityEditor.MenuItem("Window/UI Toolkit/ToolCustomEditor")]
    public static void Init()
    {
        EditorWindow wnd = GetWindow(typeof(ToolCustomEditor));
        wnd.titleContent = new GUIContent("ToolCustomEditor");
    }

    private void OnGUI()
    {
        BeginWindows();
        Rect windowRect = new Rect(100, 100, 200, 200);
        windowRect = GUILayout.Window(1, windowRect, DoWindow, "Hola caracola");

        Rect windowRect2 = new Rect(0, 150, 200, 200);
        windowRect2 = GUILayout.Window(2, windowRect2, DoWindow, "Hola caracola");

        EndWindows();
    }

    //public void CreateGUI()
    //{
    //    BeginWindows();

    //    Rect windowRect = new Rect(100, 100, 200, 200);

    //    windowRect = GUILayout.Window(1, windowRect, DoWindow, "Hola caracola");

    //    var allObjectGuids = AssetDatabase.FindAssets("t:Sprite");
    //    var allObjects = new List<Sprite>();
    //    foreach (var guid in allObjectGuids)
    //    {
    //        allObjects.Add(AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)));
    //    }

    //    // Create a two-pane view with the left pane being fixed.
    //    var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

    //    // Add the view to the visual tree by adding it as a child to the root element.
    //    rootVisualElement.Add(splitView);

    //    // A TwoPaneSplitView needs exactly two child elements.
    //    var leftPane = new UnityEngine.UIElements.ListView();
    //    splitView.Add(leftPane);
    //    m_rightPane = new VisualElement();
    //    splitView.Add(m_rightPane);

    //    // Initialize the list view with all sprites' names
    //    leftPane.makeItem = () => new UnityEngine.UIElements.Label();
    //    leftPane.bindItem = (item, index) => { (item as UnityEngine.UIElements.Label).text = allObjects[index].name; };
    //    leftPane.itemsSource = allObjects;

    //    // React to the user's selection
    //    leftPane.selectionChanged += OnSpriteSelectionChange;

    //    //Draw OpenGL Window in a new thread
    //    thread = new Thread(DrawWindow);
    //    thread.Start();

    //    EndWindows();
    //}

    void DoWindow(int unusedWindowID)
    {
        GUILayout.Button("A");
        GUI.DragWindow();
    }

    void DrawWindowGL()
    {
        thread = new Thread(DrawWindow);
        thread.Start();
    }

    private void OnDestroy()
    {
        //Destroy thread
        thread.Abort();
    }

    private void OnSpriteSelectionChange(IEnumerable<object> selectedItems)
    {
        //Clear all previous content from the pane.
        m_rightPane.Clear();
        CreateCanvas();
    }

    private void CreateCanvas()
    {
        //using (Bitmap bitmap = new Bitmap(100, 100))
        //{
        //    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
        //    {
        //        g.Clear(System.Drawing.Color.Red);
        //    }
        //    string path = Directory.GetCurrentDirectory();
        //    Debug.Log(path);
        //    bitmap.Save(path + "myBitmap.bmp");
        //}

        Bitmap bitmap = new Bitmap(100, 100);
        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

        Pen pen = new Pen(System.Drawing.Color.FromArgb(255, 0, 0, 0));
        g.DrawLine(pen, 20, 10, 300, 100);

        string path = Directory.GetCurrentDirectory();
        Debug.Log(path);
        bitmap.Save(path + "myBitmap.bmp");
    }


}
