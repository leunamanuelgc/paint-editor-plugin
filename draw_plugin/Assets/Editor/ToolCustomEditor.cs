using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using System.Threading;
using Unity.Jobs;

public class ToolCustomEditor : EditorWindow
{
    private VisualElement m_rightPane;

    private Thread thread;

    [DllImport("Dll2", EntryPoint = "DrawWindow")]
    public static extern void DrawWindow();

    [MenuItem("Window/UI Toolkit/ToolCustomEditor")]
    public static void ShowExample()
    {
        ToolCustomEditor wnd = GetWindow<ToolCustomEditor>();
        wnd.titleContent = new GUIContent("ToolCustomEditor");
    }

    public void CreateGUI()
    {
        var allObjectGuids = AssetDatabase.FindAssets("t:Sprite");
        var allObjects = new List<Sprite>();
        foreach (var guid in allObjectGuids)
        {
            allObjects.Add(AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)));
        }

        // Create a two-pane view with the left pane being fixed.
        var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

        // Add the view to the visual tree by adding it as a child to the root element.
        rootVisualElement.Add(splitView);

        // A TwoPaneSplitView needs exactly two child elements.
        var leftPane = new ListView();
        splitView.Add(leftPane);
        m_rightPane = new VisualElement();
        splitView.Add(m_rightPane);

        // Initialize the list view with all sprites' names
        leftPane.makeItem = () => new Label();
        leftPane.bindItem = (item, index) => { (item as Label).text = allObjects[index].name; };
        leftPane.itemsSource = allObjects;

        // React to the user's selection
        leftPane.selectionChanged += OnSpriteSelectionChange;

        thread = new Thread(DrawWindow);
        thread.Start();
    }

    private void OnDestroy()
    {
        thread.Abort();
    }

    private void OnSpriteSelectionChange(IEnumerable<object> selectedItems)
    {
        //Clear all previous content from the pane.
        m_rightPane.Clear();

    }
}
