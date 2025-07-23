using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DungeonVisualizer : MonoBehaviour
{
    public DungeonMap dungeonMap;

    public RawImage rawImage;
    
    void OnDrawGizmos()
    {
        if (dungeonMap == null || dungeonMap.rooms == null) return;

        Gizmos.color = Color.green;
        foreach (var room in dungeonMap.rooms)
        {
            Vector3 pos = new Vector3(room.basePosition.x, 0, room.basePosition.y);
            Vector3 size = new Vector3(room.size.x, 0, room.size.y);
            Gizmos.DrawWireCube(pos + size / 2, size); // 画房间
        }

        Gizmos.color = Color.cyan;
        foreach (var corridor in dungeonMap.corridors)
        {
            Vector3 start = new Vector3(corridor.start.x, 0, corridor.start.y);
            Vector3 end = new Vector3(corridor.end.x, 0, corridor.end.y);
            Gizmos.DrawLine(start, end);
        }

        {
            //Draw Map Position
            Gizmos.color = Color.white;
            Vector3 pos = new Vector3(0, 0, 0);
            Vector3 size = new Vector3(dungeonMap.roomGenRadius * 2 + dungeonMap.maxRoomSize, 0,
                dungeonMap.roomGenRadius * 2 + dungeonMap.maxRoomSize);
            Gizmos.DrawWireCube(pos, size);
        }
    }
}