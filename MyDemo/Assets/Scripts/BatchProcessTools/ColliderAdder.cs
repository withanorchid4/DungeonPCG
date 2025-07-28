using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ColliderAdder
{
    [MenuItem("Tools/批量给Prefab加MeshCollider")]
    public static void AddMeshColliders()
    {
        // 选中你的ScriptableObject
        var asset = Selection.activeObject as PrefabResource;
        if (asset == null)
        {
            Debug.LogError("请先在Project窗口选中PrefabListAsset！");
            return;
        }

        foreach (var prefabInfo in asset.prefabInfos)
        {
            if (prefabInfo == null) continue;

            // 递归所有子节点
            foreach (var mf in prefabInfo.prefab.GetComponentsInChildren<MeshFilter>(true))
            {
                var go = mf.gameObject;
                // 如果已经有Collider就跳过
                if (go.GetComponent<Collider>() != null) continue;

                // 加MeshCollider
                var mc = go.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                mc.convex = false; // 视需求而定
                EditorUtility.SetDirty(prefabInfo.prefab);
                Debug.Log($"已给 {prefabInfo.prefab.name}/{go.name} 添加MeshCollider");
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("批量添加完成！");
    }
}