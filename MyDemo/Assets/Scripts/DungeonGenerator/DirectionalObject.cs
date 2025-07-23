using UnityEngine;
using System.Collections.Generic;
public class DirectionalObject
{
    public DecorDir dir;

    public float GetRotationAngle()
    {
        float angle = 0f;
        if (dir == DecorDir.Down)
        {
            angle = 0;
        }
        else if (dir == DecorDir.Left)
        {
            angle = 90f;
        }
        else if (dir == DecorDir.Up)
        {
            angle = 180f;
        }
        else if (dir == DecorDir.Right)
        {
            angle = 270f;
        }

        return angle;
    }
}

public class AreaTypeProxy
{
    public string name;
    public List<LocalAreaRange> localAreas;

    public AreaTypeProxy(string name)
    {
        this.name = name;
        localAreas = new List<LocalAreaRange>();
    }
}

public class LocalAreaRange
{
    public Vector2Int min;
    public Vector2Int size;
}