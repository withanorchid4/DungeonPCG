using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class VectorTools
{
    public static float GetCross(Vector2 v1, Vector2 v2)
    {
        return v1.x * v2.y - v1.y * v2.x;
    }
    
    public static bool IsSegmentIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        float d1 = GetCross(q2 - q1, p1 - q1);
        float d2 = GetCross(q2 - q1, p2 - q1);
        float d3 = GetCross(p2 - p1, q1 - p1);
        float d4 = GetCross(p2 - p1, q2 - p1);

        // 严格相交
        if (d1 * d2 < 0 && d3 * d4 < 0)
            return true;

        // 端点共线特判（可选，视需求而定）
        if (d1 == 0 && OnSegment(q1, q2, p1)) return true;
        if (d2 == 0 && OnSegment(q1, q2, p2)) return true;
        if (d3 == 0 && OnSegment(p1, p2, q1)) return true;
        if (d4 == 0 && OnSegment(p1, p2, q2)) return true;

        return false;
    }
    
    public static bool IsSegmentIntersectForLine(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        float d1 = GetCross(q2 - q1, p1 - q1);
        float d2 = GetCross(q2 - q1, p2 - q1);
        float d3 = GetCross(p2 - p1, q1 - p1);
        float d4 = GetCross(p2 - p1, q2 - p1);

        // 严格相交
        if (d1 * d2 < 0 && d3 * d4 < 0)
            return true;

        return false;
    }
    
    public static bool OnSegment(Vector2 a, Vector2 b, Vector2 p)
    {
        // 判断点p是否在线段ab上（假设已经共线）
        return Mathf.Min(a.x, b.x) - 1e-6f <= p.x 
               && p.x <= Mathf.Max(a.x, b.x) + 1e-6f
               && Mathf.Min(a.y, b.y) - 1e-6f <= p.y 
               && p.y <= Mathf.Max(a.y, b.y) + 1e-6f;
    }
    
    public static Vector3 Multiply(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }
}
