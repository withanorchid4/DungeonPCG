using System;
using UnityEngine;
using System.Collections.Generic;

public class BookShelfInstancer : MonoBehaviour
{
    // [SerializeField]
    // public GameObject bookPrefab;
    //
    // [SerializeField] public Mesh bookMesh;
    // [SerializeField] public Material bookMaterial;

    [SerializeField] public Dictionary<(Mesh, Material), List<Matrix4x4> > instanceDict;
    private void Awake()
    {
        // var bookShelfMesh = bookPrefab.GetComponent<MeshFilter>();
        // bookMesh = bookShelfMesh.mesh;
        //
        // var bookShelfRenderer = bookPrefab.GetComponent<MeshRenderer>();
        // bookMaterial = bookShelfRenderer.material;
        
        //instanceDict = new Dictionary<(Mesh, Material), List<Matrix4x4>>();
    }

    public void InitializeOnEditor()
    {
        instanceDict = new Dictionary<(Mesh, Material), List<Matrix4x4>>();
    }

    private void Update()
    {
        foreach (var kvp in instanceDict)
        {
            Graphics.DrawMeshInstanced(kvp.Key.Item1, 0, kvp.Key.Item2, kvp.Value);
        }
    }

    public void ClearInstanceCache()
    {
        instanceDict.Clear();
    }
    
    public void RegisterInstance(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        var allChildrenGo = GetAllGameObjectsInPrefab(prefab);
        foreach (var child in allChildrenGo)
        {
            //先获取这个child的mesh
            //在editor中绑定位置处实例化测试
            var meshFilter = child.GetComponent<MeshFilter>();
            var meshRenderer = child.GetComponent<MeshRenderer>();
            if (meshFilter != null && meshRenderer != null && child.activeSelf == true)
            {
                var mesh = meshFilter.sharedMesh;
                var mat = meshRenderer.sharedMaterial;
                //若没有启用instancing
                if (mat.enableInstancing == false)
                {
                    mat.enableInstancing = true;
                    Debug.Log(mat.name + "已启用instancing");
                }
                if (!instanceDict.ContainsKey((mesh, mat)))
                {
                    instanceDict.Add((mesh, mat), new List<Matrix4x4>());
                }
                //prefab的transform叠加child的transform得到最终的transform
                //Matrix4x4 localMatrix = Matrix4x4.TRS(child.transform.localPosition, child.transform.localRotation, child.transform.localScale);
                Matrix4x4 localMatrix = child.transform.localToWorldMatrix;
                Matrix4x4 instanceMatrix = Matrix4x4.TRS(position, rotation, scale);
                Matrix4x4 worldMatrix = instanceMatrix * localMatrix;

                instanceDict[(mesh, mat)].Add(worldMatrix);

            }
            else
            {
                Debug.LogError("没有找到meshfilter或者meshrenderer，go名: " + child.name);
            }
        }
    }

    public void DisableAllMeshRenderer(GameObject go)
    {
        var allChildrenGo = GetAllGameObjectsInPrefab(go);
        foreach (var child in allChildrenGo)
        {
            if(child.CompareTag("Key"))
                continue;
            var meshRenderer = child.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }
        }
    }

    public void PrintInstanceInfo()
    {
        foreach (var kvp in instanceDict)
        {
            Debug.Log("mesh: " + kvp.Key.Item1.name + ", mat: " + kvp.Key.Item2.name + ", count: " + kvp.Value.Count);
            foreach (var matrix in kvp.Value)
            {
                Debug.Log(matrix);
            }
        }
    }

    public static List<GameObject> GetAllGameObjectsInPrefab(GameObject prefab)
    {
        var allGameObjects = new List<GameObject>();
        // 添加当前 Prefab 根节点
        allGameObjects.Add(prefab);
        // 递归添加子节点
        AddChildrenRecursively(prefab.transform, allGameObjects);
        return allGameObjects;
    }

    private static void AddChildrenRecursively(Transform parent, List<GameObject> list)
    {
        foreach (Transform child in parent)
        {
            // 确保不重复添加（理论上不会发生，防御性代码）
            if (!list.Contains(child.gameObject))
            {
                list.Add(child.gameObject);
            }
            // 递归处理子物体
            AddChildrenRecursively(child, list);
        }
    }
}