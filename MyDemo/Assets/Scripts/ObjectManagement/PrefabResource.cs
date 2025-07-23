using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "PrefabBind", menuName = "Configs/Prefab Bind Config")]
public class PrefabResource : ScriptableObject
{
    public PrefabInfo[] prefabInfos;
}

[System.Serializable]
public class PrefabInfo
{
    public GameObject prefab;
    public Vector3 basePosition;
    public Vector3 size;
    public PrefabType type;
}

public enum PrefabType
{
    OnFloor, //贴在地面上
    OnWall,  //贴在墙上
    OnCeiling,  //贴在天花板上
    ObstacleOnFloor,  //障碍物
    OnDesk  //贴在桌上
}

public static class PrefabTypeHeightMap
{
    public static readonly Dictionary<PrefabType, float> TypeToHeight = new Dictionary<PrefabType, float>
    {
        { PrefabType.OnFloor, 0.1f },
        { PrefabType.OnWall, 3f },
        { PrefabType.OnCeiling, 5f },
        { PrefabType.ObstacleOnFloor, 0f },
        { PrefabType.OnDesk, 1f }
    };
}
