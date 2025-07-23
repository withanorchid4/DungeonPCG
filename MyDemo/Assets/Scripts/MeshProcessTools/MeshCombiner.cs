using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshCombiner
{
    public static Mesh CombineMesh(Vector3[] position, Mesh baseMesh, Vector3 meshPosition, Vector3 meshScale, Quaternion meshRotation, bool isWall)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        for (int i = 0; i < position.Length; i++)
        {
            var offset = position[i];
            
            //Debug.Log("offset: " + offset);

            foreach (var vertex in baseMesh.vertices)
            {
                vertices.Add(offset + meshRotation * VectorTools.Multiply(vertex, meshScale) + meshPosition);
                //Debug.Log("当前点的位置" + vertex);
            }

            for (int j = 0; j < baseMesh.triangles.Length; j+=3)
            {
                List<Vector3> tempVertices = new List<Vector3>();
                for (int k = 0; k < 3; k++)
                {
                    tempVertices.Add(vertices[baseMesh.triangles[j + k]]);
                }

                if (isWall || !isThisTriangleVertical(tempVertices))
                {
                    for (int k = 0; k < 3; k++)
                    {
                        indices.Add(baseMesh.triangles[j + k] + i * baseMesh.vertexCount);
                    }
                }
            }

            foreach (var u in baseMesh.uv)
            {
                uvs.Add(u);
            }
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    public static GameObject CreateCombinedObject(string name, Mesh mesh, Material mat)
    {
        GameObject obj = new GameObject(name);
        
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        
        MeshCollider mc = obj.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;
        //mc.convex = true;  //设置为凸包
        return obj;
    }

    private static bool isThisTriangleVertical(List<Vector3> vertices)
    {
        var vec1 = vertices[1] - vertices[0];
        var vec2 = vertices[2] - vertices[0];
        var cross = Vector3.Cross(vec1, vec2);
        return Mathf.Abs(cross.y) < 0.001f;
    }
}
