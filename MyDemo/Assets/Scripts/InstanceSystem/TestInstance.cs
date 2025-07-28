using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;

using UnityEngine;

using UnityEditor;
using Object = System.Object;
using Random = UnityEngine.Random;

public class TestInstance : EditorWindow
{
    private BookShelfInstancer bookShelfInstancer;
    
    private GameObject prefabToPlace;

    public void OnEnable()
    {
        bookShelfInstancer = FindObjectOfType<BookShelfInstancer>();
        if (bookShelfInstancer == null)
        {
            Debug.LogError("未找到BookShelfInstancer");
        }
    }
    public void OnGUI()
    {
        GUILayout.Label("测试用instance系统绘制物体");
        // 添加Prefab拖拽字段
        prefabToPlace = (GameObject)EditorGUILayout.ObjectField(
            "要放置的Prefab", 
            prefabToPlace, 
            typeof(GameObject), 
            false); // false表示不允许场景对象，只允许Prefab
        
        
        if (GUILayout.Button("注册此物体到instrance中"))
        {
            bookShelfInstancer.InitializeOnEditor();
            bookShelfInstancer.RegisterInstance(prefabToPlace, Vector3.zero, Quaternion.identity, Vector3.one);
        }

        if (GUILayout.Button("清除instance"))
        {
            bookShelfInstancer.ClearInstanceCache();
        }

        if (GUILayout.Button("打印instance信息"))
        {
            bookShelfInstancer.PrintInstanceInfo();
        }
        
        
    }
    
    [MenuItem("Tools/2D地牢相关/测试Instance")]
    public static void OpenWindow()
    {
        var exist = FindObjectOfType<TestInstance>();
        if (exist == null)
        {
            exist = GetWindow<TestInstance>("instance测试");
        }
        exist.Show();
    }
}
