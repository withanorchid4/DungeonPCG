using UnityEditor;
using UnityEngine;

public class WFCEditorWindow : EditorWindow
{
    private WFCMain wfc;
    private GridGizmoDrawer gridDrawer;

    public int seed;

    [MenuItem("Tools/WFC Controller")]
    public static void ShowWindow()
    {
        GetWindow<WFCEditorWindow>("WFC Controller");
    }

    void OnGUI()
    {
        gridDrawer = EditorGUILayout.ObjectField("Grid Drawer", gridDrawer, typeof(GridGizmoDrawer), true) as GridGizmoDrawer;

        seed = EditorGUILayout.IntField("随机种子", seed);
        
        if (GUILayout.Button("Initialize Randomly"))
        {
            UnityEngine.Random.InitState(seed);
            if (gridDrawer != null)
            {
                wfc = new WFCMain(8, 8);
                gridDrawer.wfc = wfc;
                wfc.InitializeRandomly();
            }
        }

        
        if (GUILayout.Button("Single Iteration"))
        {
            if (wfc != null) wfc.Iterate();
            UnityEditor.SceneView.RepaintAll();
        }
        if (GUILayout.Button("Complete All"))
        {
            if (wfc != null) wfc.DoMagic();
        }
    }
}