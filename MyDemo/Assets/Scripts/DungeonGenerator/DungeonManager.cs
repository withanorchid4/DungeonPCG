using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

public class DungeonManager : MonoBehaviour
{
    private DungeonMap dungeonMap;

    void Awake() //之前是Start
    {
        // GraphicsSettings.useScriptableRenderPipelineBatching = false;
        dungeonMap = new DungeonMap();
        dungeonMap.Init(10, 12, 4, 8, false);

        var instancer = FindObjectOfType<BookShelfInstancer>();
        if (instancer != null)
        {
            instancer.instanceDict = new Dictionary<(Mesh, Material), List<Matrix4x4>>();
        }
        else
        {
            Debug.LogError("BookShelfInstancer not exists");
        }
        
        var visualizer = FindObjectOfType<DungeonVisualizer>();
        if (visualizer == null)
        {
            GameObject go = new GameObject("DungeonVisualizer");
            visualizer = go.AddComponent<DungeonVisualizer>();
        }
        
        visualizer.dungeonMap = dungeonMap;
        
        dungeonMap.ExportInfoToGrid();
        
        Texture2D texture = dungeonMap.GridToTexture(true);
        visualizer.rawImage.texture = texture;
        
        dungeonMap.Generate3DSceneByPrefab();
        
        dungeonMap.GenerateDecorInScene();
        
        dungeonMap.InstantiatePlayer();
    }
}
