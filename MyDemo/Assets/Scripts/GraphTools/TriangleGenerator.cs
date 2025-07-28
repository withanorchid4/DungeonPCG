using System.Collections.Generic;
using DelaunatorSharp;
using UnityEngine;
using System.Linq;

public class TriangleGenerator : MonoBehaviour
{
    public List<Vector2> points = new List<Vector2>();
    
    void Start()
    {
        // 1. 准备二维点集
        // points = ... 填充点集数据
        AddRandomPoints(4, 10);

        for (int i = 0; i < points.Count; i++)
        {
            Debug.Log($"点 {i}: ({points[i].x}, {points[i].y})");
        }
        // 2. 转换为IPoint数组（修复后的版本）
        IPoint[] iPoints = points.Select(p => (IPoint)new Point(p.x, p.y)).ToArray();
        
        // 3. 创建Delaunator实例进行三角剖分
        Delaunator delaunator = new Delaunator(iPoints);
        
        // 4. 获取三角形索引
        int[] triangles = delaunator.Triangles;
        
        // 5. 使用三角形数据
        Debug.Log($"生成三角形数量: {triangles.Length / 3}");

        // 6. 输出三角形数据
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Debug.Log($"三角形 {i / 3}: " + triangles[i] + ", " + triangles[i + 1] + ", " + triangles[i + 2]);
        }
    }
    
    // 示例：添加随机点
    void AddRandomPoints(int count, float range)
    {
        for (int i = 0; i < count; i++)
        {
            points.Add(new Vector2(
                Random.Range(-range, range),
                Random.Range(-range, range)
            ));
        }
    }
}