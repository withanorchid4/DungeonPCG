using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    private DungeonMap dungeonMap;

    void Start()
    {
        dungeonMap = new DungeonMap();
        dungeonMap.Init(10, 12, 4, 8, false);
        
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
