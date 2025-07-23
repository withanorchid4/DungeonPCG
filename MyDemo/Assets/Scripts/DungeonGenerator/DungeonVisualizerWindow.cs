using System;
using System.Collections.Generic;

using UnityEngine;

using UnityEditor;
using Object = System.Object;

public class DungeonVisualizerWindow : EditorWindow
{
    public DungeonMap generatedMap;

    public DungeonVisualizer visualizer;

    private int roomCount;
    private int radius;
    private int minsize;
    private int maxsize;

    private bool randomRoom;
    public void OnGUI()
    {
        GUILayout.Label("地牢可视化窗口");
        roomCount = EditorGUILayout.IntField("房间数量", roomCount);
        radius = EditorGUILayout.IntField("半径", radius);
        minsize = EditorGUILayout.IntField("最小尺寸", minsize);
        maxsize = EditorGUILayout.IntField("最大尺寸", maxsize);
        randomRoom = EditorGUILayout.Toggle("是否生成指定房间", randomRoom);
        
        if (GUILayout.Button("新建一个地牢并可视化"))
        {
            GenerateAndVisualizeDungeon(randomRoom);
        }

        if (GUILayout.Button("导出地图为一张纹理图"))
        {
            generatedMap.ExportInfoToGrid();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("展示这张纹理"))
        {
            Texture2D texture = generatedMap.GridToTexture(true);
            visualizer.rawImage.texture = texture;
            // if (texture != null)
            // {
            //     float aspect = (float)texture.width / texture.height;
            //     float maxWidth = position.width - 20;
            //     float maxHeight = position.height - 200;
            //     float drawWidth = maxWidth;
            //     float drawHeight = drawWidth / aspect;
            //     if (drawHeight > maxHeight)
            //     {
            //         drawHeight = maxHeight;
            //         drawWidth = drawHeight * aspect;
            //     }
            //     Rect texRect = new Rect(10, 180, drawWidth, drawHeight);
            //     EditorGUI.DrawPreviewTexture(texRect, texture);
            // }
        }

        if (GUILayout.Button("获取门的位置并显示这张纹理"))
        {
            generatedMap.ShowWallPositionByRoomInfo();
            Texture2D texture = generatedMap.GridToTexture(true);
            visualizer.rawImage.texture = texture;
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("todo: 构建地牢Mesh"))
        {
            //待实现
            generatedMap.Generate3DSceneByPrefab();
        }

        if (GUILayout.Button("放置装饰品"))
        {
            generatedMap.GenerateDecorInScene();
        }

        if (GUILayout.Button("初始化角色位置并实例化"))
        {
            generatedMap.InstantiatePlayer();
        }
        
    }
    
    void GenerateAndVisualizeDungeon(bool random)
    {
        // 1. 生成地牢数据
        generatedMap = new DungeonMap();
        generatedMap.Init(roomCount, radius, minsize, maxsize, random);

        // 2. 查找或创建DungeonVisualizer对象
        visualizer = FindObjectOfType<DungeonVisualizer>();
        if (visualizer == null)
        {
            GameObject go = new GameObject("DungeonVisualizer");
            visualizer = go.AddComponent<DungeonVisualizer>();
        }

        // 3. 赋值
        visualizer.dungeonMap = generatedMap;

        // 4. 选中可视化对象，方便Scene视图查看
        Selection.activeGameObject = visualizer.gameObject;

        // 5. 刷新Scene视图
        SceneView.RepaintAll();
    }
    
    [MenuItem("Tools/2D地牢相关/Dungeon Visualizer")]
    public static void OpenWindow()
    {
        var exist = FindObjectOfType<DungeonVisualizerWindow>();
        if (exist == null)
        {
            exist = GetWindow<DungeonVisualizerWindow>("地牢可视化窗口");
        }
        exist.Show();
    }
}
