using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
// using System.Numerics;
using UnityEngine;
using Unity.AI.Navigation;

using UnityEditor;

public enum RoomType {Entry, Blank, Key, Puzzle, Monster, Exit}
public enum DoorType {None, Exit, Locked}
public enum GridCellType {None, Floor, Corridor, CorridorOnFloor, Door, Wall, Desk, Occupied, XPWall, XNWall, ZPWall, ZNWall, RoomCorner, Exit, OnTesting}
public enum DecorDir {None, Left, Right, Up, Down}

public class CellInfo
{
    public GridCellType cellType;
    public int roomID;
}
public class RoomData
{
    public Vector2Int basePosition;  //房间左下角坐标
    public Vector2Int size;
    public RoomType roomType;
    
    public List<AreaTypeProxy> areas;

    public List<DecoratorData> decors;
    
    public uint roomID;
    
    public Vector2Int maxPosition => basePosition + size;
    
    public Vector2 center => basePosition + size / 2;
    
    public bool IsRoomsInsected(RoomData otherRoom)
    {
        if (basePosition.x > otherRoom.maxPosition.x 
            || otherRoom.basePosition.x > maxPosition.x 
            || basePosition.y > otherRoom.maxPosition.y 
            || otherRoom.basePosition.y > maxPosition.y)
        {
            return false;
        }
        return true;
    }

    public bool IsCorridorInsected(CorridorData corridor)
    {
        Vector2 p1 = corridor.start;
        Vector2 p2 = corridor.end;

        // 端点在AABB内
        if (IsPointInside(p1) || IsPointInside(p2))
            return true;
        
        Vector2 bl = basePosition;
        Vector2 br = new Vector2(maxPosition.x, basePosition.y);
        Vector2 tr = maxPosition;
        Vector2 tl = new Vector2(basePosition.x, maxPosition.y);

        // 四条边
        if (VectorTools.IsSegmentIntersect(p1, p2, bl, br)) return true; // bottom
        if (VectorTools.IsSegmentIntersect(p1, p2, br, tr)) return true; // right
        if (VectorTools.IsSegmentIntersect(p1, p2, tr, tl)) return true; // top
        if (VectorTools.IsSegmentIntersect(p1, p2, tl, bl)) return true; // left

        return false;
    }

    public bool DistFromCorridorIsOne(CorridorData corridor) //为了防止文档中的bug 2
    {
        var minX = basePosition.x;
        var maxX = maxPosition.x;
        var minY = basePosition.y;
        var maxY = maxPosition.y;
        
        Vector2 p1 = corridor.start;
        Vector2 p2 = corridor.end;
        bool isHorizontal = p1.y == p2.y;
        if (isHorizontal)
        {
            if(Mathf.Max(p1.x, p2.x) < minX || Mathf.Min(p1.x, p2.x) > maxX)
                return false;
            if(p1.y + 1 == minY || p1.y == maxY)
                return true;
        }
        else
        {
            if(Mathf.Max(p1.y, p2.y) < minY || Mathf.Min(p1.y, p2.y) > maxY)
                return false;
            if(p1.x + 1 == minX || p1.x == maxX)
                return true;
        }

        return false;
    }
    
    public bool IsPointInside(Vector2 point) //重合也算在里面
    {
        return point.x >= basePosition.x && point.x <= maxPosition.x && point.y >= basePosition.y && point.y <= maxPosition.y;
    }
}



public class DoorData : DirectionalObject
{
    public Vector2 positionInGrid;
    public int doorID;
    public bool enable;
    public bool isDoorOpen;
    public bool isExit;
}

public class CorridorData
{
    public Vector2 start;
    public Vector2 end;

    public bool IsCorridorInsected(CorridorData otherCorridor)
    {
        var a = this.start;
        var b = this.end;
        var c = otherCorridor.start;
        var d = otherCorridor.end;

        return VectorTools.IsSegmentIntersectForLine(a, b, c, d);
    }

    public bool IsCorridorNeighbor(CorridorData otherCorridor)
    {
        //判断是否是相邻的走廊，即同向时，只差一个单位，且有一段重合
        bool thisHorizontal = start.y == end.y;
        bool otherHorizontal = otherCorridor.start.y == otherCorridor.end.y;
        if (thisHorizontal != otherHorizontal) return false;
        if (thisHorizontal)
        {
            if (Mathf.Abs(start.y - otherCorridor.start.y) != 1)
            {
                return false;
            }
            if(Mathf.Min(otherCorridor.start.x, otherCorridor.end.x) < Mathf.Max(start.x, end.x) &&
               Mathf.Max(otherCorridor.start.x, otherCorridor.end.x) > Mathf.Min(start.x, end.x))
            {
                return true;
            }
        }
        else
        {
            if (Mathf.Abs(start.x - otherCorridor.start.x) != 1)
            {
                return false;
            }

            if (Mathf.Min(otherCorridor.start.y, otherCorridor.end.y) < Mathf.Max(start.y, end.y) &&
                Mathf.Max(otherCorridor.start.y, otherCorridor.end.y) > Mathf.Min(start.y, end.y))
            {
                return true;
            }
        }

        return false;
    }
}

public class DungeonMap
{
    public List<RoomData> rooms;
    public List<CorridorData> corridors;
    public uint mapID;
    public int roomCount;

    public int roomGenRadius;
    public int minRoomSize;
    public int maxRoomSize;
    public int offset; // 从房间坐标到标准grid的偏移

    public CellInfo[,] grid;
    public CellInfo[,] greaterGrid;

    public const float ceilHeight = 4.8f;
    

    public List<DoorData> potentialDoors;

    MatrixGraph graph;
    //configs
    private RoomConfig blankRoonConfig;
    private RoomConfig keyRoomConfig;
    
    //玩家出生，出口，怪物生成信息
    public Vector2Int playerBirthPos;
    public Vector2Int exitPos;
    public Vector2Int monsterBirthPos;

    public void Init(int roomCount, int roomGenRadius, int minRoomSize, int maxRoomSize, bool randomRoom)
    {
        blankRoonConfig = AssetDatabase.LoadAssetAtPath<RoomConfig>("Assets/Configs/BlankRoomConfig.asset");
        keyRoomConfig = AssetDatabase.LoadAssetAtPath<RoomConfig>("Assets/Configs/KeyRoomConfig.asset");
        offset = roomGenRadius + maxRoomSize;
        rooms = new List<RoomData>();
        corridors = new List<CorridorData>();
        potentialDoors = new List<DoorData>();
        this.roomCount = roomCount;
        this.roomGenRadius = roomGenRadius;
        this.minRoomSize = minRoomSize;
        this.maxRoomSize = maxRoomSize;
        
        var grisSize = roomGenRadius * 2 + maxRoomSize * 2;
        this.grid = new CellInfo[grisSize, grisSize];  // 0:空地，1:房间，2:走廊，3:门，4:墙，5：宝箱
        for (int i = 0; i < grisSize; i++)
        {
            for (int j = 0; j < grisSize; j++)
            {
                grid[i, j] = new CellInfo();
                grid[i, j].cellType = GridCellType.None;
                grid[i, j].roomID = -1;
            }
        }
        
        GenerateRooms(randomRoom);
        GenerateCorridors();
    }

    #region 生成房间和走廊
    private void GenerateRooms(bool random)  //方案一：简单地随机生成一些矩形
    {
        Debug.Log("开始生成房间信息");
        if (random)
        {
            // Vector2Int center1 = new Vector2Int(-2, 4);
            // Vector2Int center2 = new Vector2Int(-2, 17);
            // Vector2Int size1 = new Vector2Int(6, 6);
            // Vector2Int size2 = new Vector2Int(3, 4);
            // RoomData room1 = new RoomData();
            // room1.basePosition = center1 - size1 / 2;
            // room1.size = size1;
            // room1.roomType= RoomType.Key;
            // room1.roomID = 0;
            // room1.decors = new List<DecoratorData>();
            // room1.roomGrid = new bool[size1.x, size1.y];
            // rooms.Add(room1);
            // RoomData room2 = new RoomData();
            // room2.basePosition = center2 - size2 / 2;
            // room2.size = size2;
            // room2.roomType= RoomType.Blank;
            // room2.roomID = 1;
            // room2.decors = new List<DecoratorData>();
            // room2.roomGrid = new bool[size2.x, size2.y];
            // rooms.Add(room2);
            Vector2Int center = new Vector2Int(0, 0);
            Vector2Int size = new Vector2Int(4, 4);
            RoomData room = new RoomData();
            room.basePosition = center - size / 2;
            room.size = size;
            room.roomType= RoomType.Key;
            room.roomID = 0;
            room.decors = new List<DecoratorData>();
            rooms.Add(room);
            Debug.Log("生成房间信息完成");
            graph = new MatrixGraph(1);
            return;
        }
        uint roomIndex = 0;
        int retryCount = 0;
        while (roomIndex < roomCount && retryCount < 100)
        {
            Vector2Int center = new Vector2Int(Random.Range(-roomGenRadius, roomGenRadius), Random.Range(-roomGenRadius, roomGenRadius));
            Vector2Int size = new Vector2Int(Random.Range(minRoomSize, maxRoomSize), Random.Range(minRoomSize, maxRoomSize));
            RoomData room = new RoomData();
            room.basePosition = center - size / 2;
            room.size = size; //int 转 float
            if (IsNewRoomValid(room))
            {
                // room.roomType= roomIndex % 2 == 0 ? RoomType.Key : RoomType.Blank;
                room.roomType = RandomProcess.GetRandomRoomType();
                room.roomID = roomIndex++;
                room.decors = new List<DecoratorData>();
                rooms.Add(room);
                retryCount = 0;
            }
            retryCount ++;
        }
        
        graph = new MatrixGraph(roomCount);
    }

    private void GenerateCorridors()
    {
        Debug.Log("开始生成走廊信息");
        if (rooms == null || rooms.Count < 2) return;
        System.Random rng = new System.Random();

        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                // if (i + 1 != j && rng.Next(0, 4) == 0)
                // {
                //     continue;
                // }
                var preRoomCenter = rooms[i].center;
                var curRoomCenter = rooms[j].center;
                bool horizontalFirst = rng.Next(0, 2) == 0;

                Vector2 corner;
                if (horizontalFirst)
                {
                    corner = new Vector2(preRoomCenter.x, curRoomCenter.y);
                }
                else
                {
                    corner = new Vector2(curRoomCenter.x, preRoomCenter.y);
                }

                List<CorridorData> tempCorridors = new List<CorridorData>();

                //判断是否只需要一条走廊就可以连接两个房间
                if (!rooms[i].IsPointInside(corner) || true)  //把房间里的走廊作为过道标注出来
                {
                    //Debug.Log("需要两条走廊才能连接两个房间1");
                    tempCorridors.Add(new CorridorData { start = preRoomCenter, end = corner });
                }

                if (!rooms[j].IsPointInside(corner) || true) //把房间里的走廊作为过道标注出来
                {
                    //Debug.Log("需要两条走廊才能连接两个房间2");
                    tempCorridors.Add(new CorridorData { start = corner, end = curRoomCenter });
                }

                bool isThisCorridorValid = true;
                foreach (var corridor in tempCorridors)
                {
                    if (!IsnewCorridorValid(corridor, i, j))
                    {
                        isThisCorridorValid = false;
                        break;
                    }
                }

                //Debug.Log("当前走廊有效：" + isThisCorridorValid);
                if (isThisCorridorValid)
                {
                    corridors.AddRange(tempCorridors);
                    //构建图中的边
                    float pathLength = Mathf.Abs(preRoomCenter.x - curRoomCenter.x) + Mathf.Abs(preRoomCenter.y - curRoomCenter.y);
                    graph.AddEdge(i, j, 1);
                }
            }
        }
        graph.BuildDistances();
        //Debug.Log("Count Corridot" + corridors.Count);
    }

    private bool IsNewRoomValid(RoomData newRoom)
    {
        //if (newRoom.size)
        foreach (var room in rooms)
        {
            if (newRoom.IsRoomsInsected(room))
            {
                return false;
            }
        }
        return true;
    }

    private bool IsnewCorridorValid(CorridorData newCorridor, int preIndex, int curIndex)
    {
        foreach (var corridor in corridors)
        {
            if (corridor.IsCorridorInsected(newCorridor))
                return false;
            if (corridor.IsCorridorNeighbor(newCorridor)) //避免同一扇墙，一个地方有两扇门
                return false;
        }
        
        for (int i = 0; i < rooms.Count; i++)
        {
            //Debug.Log("tak!");
            if (rooms[i].DistFromCorridorIsOne(newCorridor)) //和每个房间的距离都不能为1，都不能贴住，否则会有文档中的bug 2, 这个优化相当于把每个房间都扩大了一圈来限制走廊的分布
                return false;
            if (i == preIndex || i == curIndex)
                continue;
            if (rooms[i].IsCorridorInsected(newCorridor))
                return false;
        }

        return true;
    }
    
    #endregion
    
    #region 导出信息到网格

    public void ExportInfoToGrid()
    {
        // var offset = roomGenRadius + maxRoomSize;
        foreach (var room in rooms)
        {
            for (int x = (int)room.basePosition.x; x < room.maxPosition.x; x++)
            {
                for (int y = (int)room.basePosition.y; y < room.maxPosition.y; y++)
                {
                    //grid[x, y] = 1;
                    grid[x + offset, y + offset].cellType = GridCellType.Floor;
                    grid[x + offset, y + offset].roomID = (int)room.roomID;
                }
            }
        }

        foreach (var corridor in corridors)
        {
            int x0 = Mathf.RoundToInt(corridor.start.x);
            int y0 = Mathf.RoundToInt(corridor.start.y);
            int x1 = Mathf.RoundToInt(corridor.end.x);
            int y1 = Mathf.RoundToInt(corridor.end.y);
        
            if (y0 == y1)
            {
                int minX = Mathf.Min(x0, x1);
                int maxX = Mathf.Max(x0, x1);
                for (int i = minX; i <= maxX; i++)
                {
                    if(grid[i + offset, y0 + offset].cellType != GridCellType.Floor && grid[i + offset, y0 + offset].cellType != GridCellType.CorridorOnFloor) //把房间里的走廊作为过道标注出来
                    {
                        grid[i + offset, y0 + offset].cellType = GridCellType.Corridor;
                        //grid[i + offset, y0 + offset].roomID = -1;
                    }
                    else
                    {
                        grid[i + offset, y0 + offset].cellType = GridCellType.CorridorOnFloor;
                        //grid[i + offset, y0 + offset].roomID = -1;
                    }
                }
            }
            else if (x0 == x1)
            {
                int minY = Mathf.Min(y0, y1);
                int maxY = Mathf.Max(y0, y1);
                for (int i = minY; i <= maxY; i++)
                {
                    if(grid[x0 + offset, i + offset].cellType != GridCellType.Floor && grid[x0 + offset, i + offset].cellType != GridCellType.CorridorOnFloor) //把房间里的走廊作为过道标注出来
                    {
                        grid[x0 + offset, i + offset].cellType = GridCellType.Corridor;
                        //grid[x0 + offset, i + offset].roomID = -1;
                    }
                    else
                    {
                        grid[x0 + offset, i + offset].cellType = GridCellType.CorridorOnFloor;
                        //grid[x0 + offset, i + offset].roomID = -1;
                    }
                }
            }
        }
        
        //找门的位置：corridor的两端都是floor的话，这里就是门，不一定，现在的corridor会延申到room里面，得加一个判断，当前判断为door的地方，周围8个点有none，才是真正的门
        for (int i = grid.GetLength(0) - 1; i > 0; i--)
        {
            for (int j = grid.GetLength(1) - 1; j > 0; j--)
            {
                if (grid[i, j].cellType == GridCellType.CorridorOnFloor)
                {
                    if(CouldThisCellBeDoor(i, j, out DecorDir dir))
                    {
                        if (dir == DecorDir.None)
                        {
                            Debug.LogError("找到门，但是方向错误！");
                            continue;
                        }
                        grid[i, j].cellType = GridCellType.Door;
                        potentialDoors.Add(new DoorData { positionInGrid = new Vector2(i, j), dir = dir, doorID = potentialDoors.Count + 1, isDoorOpen = false, enable = false, isExit = false});
                    }
                    
                }
            }
        }
        
        //生成门完成，开始构造每个房间的area
        foreach (var room in rooms)
        {
            room.areas = new List<AreaTypeProxy>();
            if (room.roomType == RoomType.Blank)
            {
                continue;
            }
            else if (room.roomType == RoomType.Key)
            {
                var bornArea = new AreaTypeProxy("Born");
                
            }
            else if (room.roomType == RoomType.Puzzle)
            {
                
            }
            else if (room.roomType == RoomType.Monster)
            {
                
            }
            
        }
        
        
        //上采样grid
        greaterGrid = new CellInfo[grid.GetLength(0) * 2, grid.GetLength(0) * 2];
        for (int i = 0; i < greaterGrid.GetLength(0); i++)
        {
            for (int j = 0; j < greaterGrid.GetLength(1); j++)
            {
                greaterGrid[i, j] = new CellInfo();
                greaterGrid[i, j].cellType = GridCellType.None;
                greaterGrid[i, j].roomID = -1;
            }
        }
        
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                // greaterGrid[i * 2, j * 2] = grid[i, j];
                greaterGrid[i * 2, j * 2].cellType = grid[i, j].cellType;
                greaterGrid[i * 2, j * 2].roomID = grid[i, j].roomID;
                // greaterGrid[i * 2 + 1, j * 2] = grid[i, j];
                greaterGrid[i * 2 + 1, j * 2].cellType = grid[i, j].cellType;
                greaterGrid[i * 2 + 1, j * 2].roomID = grid[i, j].roomID;
                // greaterGrid[i * 2, j * 2 + 1] = grid[i, j];
                greaterGrid[i * 2, j * 2 + 1].cellType = grid[i, j].cellType;
                greaterGrid[i * 2, j * 2 + 1].roomID = grid[i, j].roomID;
                // greaterGrid[i * 2 + 1, j * 2 + 1] = grid[i, j];
                greaterGrid[i * 2 + 1, j * 2 + 1].cellType = grid[i, j].cellType;
                greaterGrid[i * 2 + 1, j * 2 + 1].roomID = grid[i, j].roomID;
            }
        }
        
        GenerateCharacterAndMonsterInitPosition(2, out var characterInitPos, out var monsterInitPos, out var exitInitPos);
        playerBirthPos = characterInitPos;
        monsterBirthPos = monsterInitPos;
        exitPos = exitInitPos;
        greaterGrid[characterInitPos.x, characterInitPos.y].cellType = GridCellType.Occupied;
        rooms[greaterGrid[monsterInitPos.x, monsterInitPos.y].roomID].roomType = RoomType.Entry;
        greaterGrid[monsterInitPos.x, monsterInitPos.y].cellType = GridCellType.Occupied;
        rooms[greaterGrid[monsterInitPos.x, monsterInitPos.y].roomID].roomType = RoomType.Monster;
        greaterGrid[exitInitPos.x, exitInitPos.y].cellType = greaterGrid[exitInitPos.x + 1, exitInitPos.y].cellType = greaterGrid[exitInitPos.x, exitInitPos.y + 1].cellType = greaterGrid[exitInitPos.x + 1, exitInitPos.y + 1].cellType = GridCellType.Exit;
        rooms[greaterGrid[exitInitPos.x, exitInitPos.y].roomID].roomType = RoomType.Exit;
        potentialDoors.Add(new DoorData { positionInGrid = new Vector2Int(exitInitPos.x / 2, exitInitPos.y / 2), dir = DecorDir.Up, doorID = potentialDoors.Count + 1, isDoorOpen = false, enable = false, isExit = true});
    }

    private bool CouldThisCellBeDoor(int i, int j, out DecorDir dir)
    {
        if (grid[i - 1, j].cellType == GridCellType.Corridor)
        {
            dir = DecorDir.Left;
            return true;
        }
        else if (grid[i + 1, j].cellType == GridCellType.Corridor)
        {
            dir = DecorDir.Right;
            return true;
        }
        else if (grid[i, j - 1].cellType == GridCellType.Corridor)
        {
            dir = DecorDir.Down;
            return true;
        }
        else if (grid[i, j + 1].cellType == GridCellType.Corridor)
        {
            dir = DecorDir.Up;
            return true;
        }
        else
        {
            dir = DecorDir.None;
            return false;
        }
    }

    public Texture2D GridToTexture(bool writeLocal)
    {

        int width = greaterGrid.GetLength(0);
        int height = greaterGrid.GetLength(1);
        Texture2D tex = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = Color.black;
                if (greaterGrid[x, y].cellType == GridCellType.Floor)
                {
                    color = new Color(0.2f, 0.8f, 0.2f); // 深绿色
                }

                if (greaterGrid[x, y].cellType == GridCellType.Corridor)
                {
                    color = new Color(0.2f, 0.8f, 0.9f); // 浅蓝色
                }

                if (greaterGrid[x, y].cellType == GridCellType.Door)
                {
                    color = new Color(1.0f, 0.8f, 0.0f); // 金色
                }

                if (greaterGrid[x, y].cellType == GridCellType.CorridorOnFloor)
                {
                    color = new Color(0.3f, 0.6f, 0.5f); // 浅绿色
                }

                if (greaterGrid[x, y].cellType == GridCellType.Occupied)
                {
                    color = Color.red;
                }
                
                //Wall
                
                if (greaterGrid[x, y].cellType == GridCellType.XPWall)
                {
                    color = new Color(0.9f, 0.5f, 0.9f); // 紫色
                }

                if (greaterGrid[x, y].cellType == GridCellType.XNWall)
                {
                    color = new Color(0.2f, 0.2f, 0.8f); // 深蓝色
                }

                if (greaterGrid[x, y].cellType == GridCellType.ZPWall)
                {
                    color = new Color(0.6f, 0.8f, 0.2f); // 灰色
                }

                if (greaterGrid[x, y].cellType == GridCellType.ZNWall)
                {
                    color = new Color(0.5f, 0.0f, 0.5f); // 深紫色
                }

                if (greaterGrid[x, y].cellType == GridCellType.Wall)
                {
                    color = Color.magenta;
                }

                if (greaterGrid[x, y].cellType == GridCellType.RoomCorner)
                {
                    color = Color.white;
                }
                
                //End Wall
                
                if (greaterGrid[x, y].cellType == GridCellType.Exit)
                {
                    color = Color.green;
                }
                
                if (greaterGrid[x, y].cellType == GridCellType.OnTesting)
                {
                    color = new Color(0, 1, 1);
                }
                

                tex.SetPixel(x, height - 1 - y, color);
            }
        }
        tex.Apply();
        //dungeonTex = tex;
        if (writeLocal)
        {
            SaveTextureToFile(tex, "Assets/dungeon.png");
        }
        return tex;
    }
    
    public void SaveTextureToFile(Texture2D tex, string path)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Debug.Log("Texture saved to: " + path);
    }
    #endregion
    
    #region 实例化prefab到场景中

    public void Generate3DSceneByPrefab()
    {
        #if UNITY_EDITOR
        GameObject floorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Configs/DecorPrefabs/Floor.prefab");
        GameObject ceilPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Configs/DecorPrefabs/Wood_ceil.prefab");
        // GameObject wallMinX = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ArtForSecondScene/Prefabs/Wall_X_Negative_New_1.prefab");
        // GameObject wallMinZ = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ArtForSecondScene/Prefabs/Wall_Z_Negative_New.prefab");
        // GameObject wallMaxX = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ArtForSecondScene/Prefabs/Wall_X_Positive_New.prefab");
        // GameObject wallMaxZ = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ArtForSecondScene/Prefabs/Wall_Z_Positive_New.prefab");
        GameObject wallMinX = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Configs/WallPrefabs/Wall_X-Collider.prefab");
        GameObject wallMinZ = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Configs/WallPrefabs/Wall_Z-Collider.prefab");
        GameObject wallMaxX = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Configs/WallPrefabs/Wall_X+Collider.prefab");
        GameObject wallMaxZ = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Configs/WallPrefabs/Wall_Z+Collider.prefab");
        #else
        GameObject prefab = Resources.Load<GameObject>("Assets/ArtForSecondScene/DungeonModularPack/Prefabs/Unit_Tile.prefab"); //todo：补全路径
        #endif

        if (floorPrefab == null)
        {
            Debug.LogError("Floor prefab not found");
        }
        
        // var prefabLoader = GameObject.Find("PrefabLoadHelper")?.GetComponent<PrefabLoadHelper>();
        // if (prefabLoader == null)
        // {
        //     Debug.LogError("PrefabLoadHelper not found");
        // }
        // else
        // {
        //     floorPrefab = prefabLoader.floorPrefab;
        //     ceilPrefab = prefabLoader.woolCeilPrefab;
        //     wallMinX = prefabLoader.wallMinXPrefab;
        //     wallMaxX = prefabLoader.wallMaxXPrefab;
        //     wallMinZ = prefabLoader.wallMinZPrefab;
        //     wallMaxZ = prefabLoader.wallMaxZPrefab;
        // }
        
        //收集地板和天花板的mesh，准备合并成一整个mesh
        List<Vector3> floorPositions = new List<Vector3>();
        List<Vector3> ceilPositions = new List<Vector3>();
        
        //收集四种墙的mesh，准备合并成一整个mesh
        List<Vector3> wallMinXPositions = new List<Vector3>();
        List<Vector3> wallMinZPositions = new List<Vector3>();
        List<Vector3> wallMaxXPositions = new List<Vector3>();
        List<Vector3> wallMaxZPositions = new List<Vector3>();
        
        for (int i = 0; i < greaterGrid.GetLength(0); i++)
        {
            for (int j = 0; j < greaterGrid.GetLength(1); j++)
            {
                var cellInfo = greaterGrid[i, j];
                Vector3 position = new Vector3(i, 0, j);

                // if (cellInfo.cellType == GridCellType.Floor || cellInfo.cellType == GridCellType.Corridor || cellInfo.cellType == GridCellType.Door || cellInfo.cellType == GridCellType.CorridorOnFloor)
                if (cellInfo.cellType != GridCellType.None)
                {
                    // Object.Instantiate(floorPrefab, position, Quaternion.identity);
                    // Object.Instantiate(ceilPrefab, position + new Vector3(0,ceilHeight,0), Quaternion.identity);
                    floorPositions.Add(position);
                    ceilPositions.Add(position + new Vector3(0,ceilHeight,0));
                    
                    var left = greaterGrid[i - 1, j].cellType;
                    var right = greaterGrid[i + 1, j].cellType;
                    var up = greaterGrid[i, j + 1].cellType;
                    var down = greaterGrid[i, j - 1].cellType;

                    Vector3 floorPosition = new Vector3(position.x, -0.05f, position.z);
                    if (left == GridCellType.None)
                    {
                        //Object.Instantiate(wallMinX, floorPosition, Quaternion.identity);
                        if(greaterGrid[i, j].cellType != GridCellType.Door)
                            greaterGrid[i, j].cellType = GridCellType.XNWall;
                        wallMinXPositions.Add(floorPosition);
                    }

                    if (right == GridCellType.None)
                    {
                        //Object.Instantiate(wallMaxX, floorPosition, Quaternion.identity);
                        if(greaterGrid[i, j].cellType != GridCellType.Door)
                            greaterGrid[i, j].cellType = GridCellType.XPWall;
                        wallMaxXPositions.Add(floorPosition);
                    }

                    if (up == GridCellType.None && greaterGrid[i, j].cellType != GridCellType.Exit)
                    {
                        //Object.Instantiate(wallMaxZ, floorPosition, Quaternion.identity);
                        if(greaterGrid[i, j].cellType != GridCellType.Door)
                            greaterGrid[i, j].cellType = GridCellType.ZPWall;
                        wallMaxZPositions.Add(floorPosition);
                    }

                    if (down == GridCellType.None)
                    {
                        //Object.Instantiate(wallMinZ, floorPosition, Quaternion.identity);
                        if(greaterGrid[i, j].cellType != GridCellType.Door)
                            greaterGrid[i, j].cellType = GridCellType.ZNWall;
                        wallMinZPositions.Add(floorPosition);
                    }
                }
            }
        }
        
        #region 获取prefab的position和scale
        //获取floor子节点的position和scale
        Vector3 floorChildPositin;
        Vector3 floorChildScale;
        Quaternion floorChildRotation;
        Transform child = floorPrefab.transform.GetChild(0);
        floorChildPositin = child.position;
        floorChildRotation = child.rotation;
        floorChildScale = child.localScale;
        
        //获取ceil子节点的position和scale
        Vector3 ceilChildPositin;
        Vector3 ceilChildScale;
        Quaternion ceilChildRotation;
        child = ceilPrefab.transform.GetChild(0);
        ceilChildPositin = child.position;
        ceilChildRotation = child.rotation;
        ceilChildScale = child.localScale;
        
        //获取wallMinX子节点的position和scale
        Vector3 wallMinXChildPositin;
        Vector3 wallMinXChildScale;
        Quaternion wallMinXChildRotation;
        child = wallMinX.transform.GetChild(0);
        wallMinXChildPositin = child.position;
        wallMinXChildRotation = child.rotation;
        wallMinXChildScale = child.localScale;
        
        //获取wallMinZ子节点的position和scale
        Vector3 wallMinZChildPositin;
        Vector3 wallMinZChildScale;
        Quaternion wallMinZChildRotation;
        child = wallMinZ.transform.GetChild(0);
        wallMinZChildPositin = child.position;
        wallMinZChildRotation = child.rotation;
        wallMinZChildScale = child.localScale;
        
        //获取wallMaxX子节点的position和scale
        Vector3 wallMaxXChildPositin;
        Vector3 wallMaxXChildScale;
        Quaternion wallMaxXChildRotation;
        child = wallMaxX.transform.GetChild(0);
        wallMaxXChildPositin = child.position;
        wallMaxXChildRotation = child.rotation;
        wallMaxXChildScale = child.localScale;
        
        //获取wallMaxZ子节点的position和scale
        Vector3 wallMaxZChildPositin;
        Vector3 wallMaxZChildScale;
        Quaternion wallMaxZChildRotation;
        child = wallMaxZ.transform.GetChild(0);
        wallMaxZChildPositin = child.position;
        wallMaxZChildRotation = child.rotation;
        wallMaxZChildScale = child.localScale;
        
        #endregion

        Mesh floorMesh = MeshCombiner.CombineMesh(floorPositions.ToArray(), floorPrefab.GetComponentInChildren<MeshFilter>().sharedMesh, floorChildPositin, floorChildScale * 1.001f, floorChildRotation, false);
        Mesh ceilMesh = MeshCombiner.CombineMesh(ceilPositions.ToArray(), ceilPrefab.GetComponentInChildren<MeshFilter>().sharedMesh, ceilChildPositin, ceilChildScale * 1.001f, ceilChildRotation, false);
        Mesh wallMinXMesh = MeshCombiner.CombineMesh(wallMinXPositions.ToArray(), wallMinX.GetComponentInChildren<MeshFilter>().sharedMesh, wallMinXChildPositin, wallMinXChildScale, wallMinXChildRotation, true);
        Mesh wallMinZMesh = MeshCombiner.CombineMesh(wallMinZPositions.ToArray(), wallMinZ.GetComponentInChildren<MeshFilter>().sharedMesh, wallMinZChildPositin, wallMinZChildScale, wallMinZChildRotation, true);
        Mesh wallMaxXMesh = MeshCombiner.CombineMesh(wallMaxXPositions.ToArray(), wallMaxX.GetComponentInChildren<MeshFilter>().sharedMesh, wallMaxXChildPositin, wallMaxXChildScale, wallMaxXChildRotation, true);
        Mesh wallMaxZMesh = MeshCombiner.CombineMesh(wallMaxZPositions.ToArray(), wallMaxZ.GetComponentInChildren<MeshFilter>().sharedMesh, wallMaxZChildPositin, wallMaxZChildScale, wallMaxZChildRotation, true);

        var floorObj = MeshCombiner.CreateCombinedObject("CombinedFloor", floorMesh,
            floorPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial);
        floorObj.transform.position = Vector3.zero;
        floorObj.transform.localScale = Vector3.one;
        floorObj.layer = LayerMask.NameToLayer("Floor");
        floorObj.isStatic = true;
        var navSurface = floorObj.AddComponent<NavMeshSurface>();
        
        navSurface.BuildNavMesh();
        
        
        var ceilObj = MeshCombiner.CreateCombinedObject("CombinedCeil", ceilMesh, ceilPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial);
        ceilObj.transform.position = Vector3.zero;
        ceilObj.transform.localScale = Vector3.one;
        
        var wallMinXObj = MeshCombiner.CreateCombinedObject("CombinedWallMinX", wallMinXMesh, wallMinX.GetComponentInChildren<MeshRenderer>().sharedMaterial);
        wallMinXObj.transform.position = Vector3.zero;
        wallMinXObj.transform.localScale = Vector3.one;
        
        var wallMinZObj = MeshCombiner.CreateCombinedObject("CombinedWallMinZ", wallMinZMesh, wallMinZ.GetComponentInChildren<MeshRenderer>().sharedMaterial);
        wallMinZObj.transform.position = Vector3.zero;
        wallMinZObj.transform.localScale = Vector3.one;
        
        var wallMaxXObj = MeshCombiner.CreateCombinedObject("CombinedWallMaxX", wallMaxXMesh, wallMaxX.GetComponentInChildren<MeshRenderer>().sharedMaterial);
        wallMaxXObj.transform.position = Vector3.zero;
        wallMaxXObj.transform.localScale = Vector3.one;
        
        var wallMaxZObj = MeshCombiner.CreateCombinedObject("CombinedWallMaxZ", wallMaxZMesh, wallMaxZ.GetComponentInChildren<MeshRenderer>().sharedMaterial);
        wallMaxZObj.transform.position = Vector3.zero;
        wallMaxZObj.transform.localScale = Vector3.one;

        ShowWallPositionByRoomInfo();

        // Object.Instantiate(floorObj, Vector3.zero, Quaternion.identity);
        // Object.Instantiate(ceilObj, Vector3.zero, Quaternion.identity);
    }

    public void GenerateDecorInScene()
    {
        foreach (var room in rooms)
        {
            //放置吊灯
            var center = room.center;
            Vector3 ceilLampPos = new Vector3(2 * (center.x + offset), ceilHeight, 2 * (center.y + offset));
            ObjectPlacementSystem.PlaceObject(ceilLampPos, "antique_light");
            
            //放置角落物体
            var leftBoundary = (room.basePosition.x + offset) * 2;
            //有边界
            var rightBoundary = (room.maxPosition.x - 1 + offset) * 2 + 1;
            //下边界
            var bottomBoundary = (room.basePosition.y + offset) * 2;
            //上边界
            var topBoundary = (room.maxPosition.y - 1 + offset) * 2 + 1;

            Vector3 cornerPos1 = new Vector3(leftBoundary + 0.5f, 0, bottomBoundary - 0.5f);
            Vector3 cornerPos2 = new Vector3(rightBoundary + 0.5f, 0, bottomBoundary - 0.5f);
            Vector3 cornerPos3 = new Vector3(leftBoundary + 0.5f, 0, topBoundary - 0.5f);
            Vector3 cornerPos4 = new Vector3(rightBoundary + 0.5f, 0, topBoundary - 0.5f);
            if(greaterGrid[leftBoundary, bottomBoundary].cellType == GridCellType.RoomCorner)
                ObjectPlacementSystem.PlaceObject(cornerPos1, "LightTall");
            if(greaterGrid[rightBoundary, bottomBoundary].cellType == GridCellType.RoomCorner)
                ObjectPlacementSystem.PlaceObject(cornerPos2, "LightTall");
            if(greaterGrid[leftBoundary, topBoundary].cellType == GridCellType.RoomCorner)
                ObjectPlacementSystem.PlaceObject(cornerPos3, "LightTall");
            if(greaterGrid[rightBoundary, topBoundary].cellType == GridCellType.RoomCorner)
                ObjectPlacementSystem.PlaceObject(cornerPos4, "LightTall");
            
            //放置宝箱
             float treasureChestRatio = Random.Range(0.0f, 1.0f);
             if (treasureChestRatio > 0.7f) //每个房间以0.3的概率放置宝箱
             {
                 Vector2Int randomPosition;
                 int retryCount = 0;
                 while(retryCount < 10)
                 {
                     //放置宝箱
                     randomPosition = new Vector2Int(Random.Range((int)room.basePosition.x, (int)room.maxPosition.x),
                         Random.Range((int)room.basePosition.y, (int)room.maxPosition.y));
                     //只允许放在边缘---------------------------放置系统的核心------------------------------------
                     if (Mathf.Abs(randomPosition.x - room.center.x) < room.size.x / 2 ||
                         Mathf.Abs(randomPosition.y - room.center.y) < room.size.y / 2)
                     {
                         retryCount++;
                         Debug.Log("尝试放置宝箱 " + retryCount);
                         if (retryCount == 10)
                         {
                             Debug.LogError("放置宝箱失败，没找到合适的位置");
                         }
                         continue;
                     }
             
                     ObjectPlacementSystem.PlaceTreasureBox(randomPosition.x, randomPosition.y); //for Debug
                     ObjectPlacementSystem.PlaceTreasureBox(2 * (randomPosition.x + offset), 2 * (randomPosition.y + offset),
                         greaterGrid);
                     break;
                 }
             }
            
             #region 放置中心区域的物体
             //放置中心区域的物体
             //地毯,沙发
             string[] areaObjs = new[] { "Carpet_X", "SofaX", "Carpet3New", "RedTable", "SmallSofa" };
             foreach (var areaObj in areaObjs)
             {
                 float placeRatio = Random.Range(0.0f, 1.0f);
                 if (placeRatio > 0.2) //todo: 在狭长的房间不放地毯
                 {
                     Vector2Int ranCenterdomPosition;
                     int retryCount = 0;
                     while (retryCount < 10)
                     {
                         //放置地毯
                         ranCenterdomPosition = new Vector2Int(
                             Random.Range((int)room.basePosition.x, (int)room.maxPosition.x),
                             Random.Range((int)room.basePosition.y, (int)room.maxPosition.y));
                         // Debug.Log(room.basePosition.x);
                         // Debug.Log(room.maxPosition.x);
                         // Debug.Log(room.basePosition.y);
                         // Debug.Log(room.maxPosition.y);
                         //只允许放在边缘---------------------------放置系统的核心------------------------------------
                         if (Mathf.Abs(ranCenterdomPosition.x - room.center.x) >= room.size.x / 3 ||
                             Mathf.Abs(ranCenterdomPosition.y - room.center.y) >= room.size.y / 3)
                         {
                             retryCount++;
                             Debug.Log("尝试放置 " + areaObj + retryCount);
                             if (retryCount == 10)
                             {
                                 Debug.LogError("放置" + areaObj + "失败，没找到合适的位置");
                             }
            
                             continue;
                         }
            
                         Debug.Log("准备放在" + ranCenterdomPosition);
                         ObjectPlacementSystem.PlaceCenterAreaObject(ranCenterdomPosition.x,
                             ranCenterdomPosition.y, areaObj); //for Debug
                         ObjectPlacementSystem.PlaceCenterAreaObject(2 * (ranCenterdomPosition.x + offset),
                             2 * (ranCenterdomPosition.y + offset), areaObj,
                             greaterGrid);
                         break;
                     }
                 }
             }
            
            #endregion
            
            #region 放置靠墙物体
            
            string[] wallObjs = new[] { "BookShelf2", "BookShelf3", "BookShelf4", "PhotoFrameDesk", "TVDesk" };
            foreach (var wallObj in wallObjs)
            {
                float placeRatio = Random.Range(0, 1f);
                if (placeRatio > 0.5)
                {
                    //既然决定要放，那么就得考虑到碰撞的请款，所以找5次
                    int tryCount = 0;
                    while (tryCount < 4)
                    {
                        //先决定放在哪一面墙
                        Vector2Int randomPos = new Vector2Int();
                        int wallIndex = Random.Range(0, 4); //0为左，逆时针
                        if (wallIndex == 0)
                        {
                            randomPos.x = 2 * (room.basePosition.x + offset);
                            randomPos.y = Random.Range(2 * (room.basePosition.y + offset) + 1,
                                2 * (room.maxPosition.y - 1 + offset));
                        }
                        else if (wallIndex == 1)
                        {
                            randomPos.y = 2 * (room.basePosition.y + offset);
                            randomPos.x = Random.Range(2 * (room.basePosition.x + offset) + 1,
                                2 * (room.maxPosition.x - 1 + offset));
                        }
                        else if (wallIndex == 2)
                        {
                            randomPos.x = 2 * (room.maxPosition.x - 1 + offset) + 1;
                            randomPos.y = Random.Range(2 * (room.basePosition.y + offset) + 1,
                                2 * (room.maxPosition.y - 1 + offset));
                        }
                        else
                        {
                            randomPos.y = 2 * (room.maxPosition.y - 1 + offset) + 1;
                            randomPos.x = Random.Range(2 * (room.basePosition.x + offset) + 1,
                                2 * (room.maxPosition.x - 1 + offset));
                        }
                        if (ObjectPlacementSystem.CouldWallObjectPlacedHere(randomPos.x, randomPos.y, wallIndex,
                                wallObj, greaterGrid))
                        {
                            //do nothing
                            break;
                        }
                        
                        //greaterGrid[randomPos.x, randomPos.y].cellType = GridCellType.OnTesting;
                        tryCount++;
                    }
                }
            }
            #endregion
        }
        

        

        //放置门，每个潜在门以0.2的概率放置，至少放置一个
        foreach (var door in potentialDoors)
        {
            float doorRatio = Random.Range(0.0f, 1.0f);
            string doorType = "";
            if (doorRatio < 0.2f)
            {
                doorType = "Closed_Door"; //关闭
                door.isDoorOpen = false;
            }
        
            else if (doorRatio >= 0.2f && doorRatio < 0.4f)
            {
                doorType = "Halfopened_Door";
                door.isDoorOpen = true;
            }
        
            else
            {
                doorType = "Opened_Door";
                door.isDoorOpen = true;
            }
        
            if (door.isExit)
            {
                //doorType = "Exit_Door";
                doorType = "Closed_Door"; //暂时用关闭的门代替
                door.isDoorOpen = false;
            }
        
            door.enable = true;
            var posInGrid = door.positionInGrid;
            var posInGreaterGrid = new Vector2(posInGrid.x * 2 + 1, posInGrid.y * 2);
            ObjectPlacementSystem.PlaceDoor(door, posInGrid.x - offset, posInGrid.y - offset, doorType);
            ObjectPlacementSystem.PlaceDoor(door, posInGreaterGrid.x, posInGreaterGrid.y, doorType, greaterGrid);
        
        }
    }
    
    #endregion
    
    #region Debug通过rooms信息来确认墙的位置

    /// <summary>
    /// 添加房间角落信息
    /// </summary>
    public void ShowWallPositionByRoomInfo()
    {
        foreach (var room in rooms)
        {
            Debug.Log("房间 " + room.roomID + " 的位置是 " + room.basePosition + " 到 " + room.maxPosition);
            //左边界
            var leftBoundary = (room.basePosition.x + offset) * 2;
            //有边界
            var rightBoundary = (room.maxPosition.x - 1 + offset) * 2 + 1;
            //下边界
            var bottomBoundary = (room.basePosition.y + offset) * 2;
            //上边界
            var topBoundary = (room.maxPosition.y - 1 + offset) * 2 + 1;
            // //左右填充
            // for (int i = bottomBoundary; i <= topBoundary; i++)
            // {
            //     greaterGrid[leftBoundary, i].cellType = GridCellType.Wall;
            //     greaterGrid[rightBoundary, i].cellType = GridCellType.Wall;
            // }
            // //上下填充
            //
            // for (int i = leftBoundary; i <= rightBoundary; i++)
            // {
            //     greaterGrid[i, bottomBoundary].cellType = GridCellType.Wall;
            //     greaterGrid[i, topBoundary].cellType = GridCellType.Wall;
            // }
            
            if(greaterGrid[leftBoundary, bottomBoundary].cellType != GridCellType.Door)
                greaterGrid[leftBoundary, bottomBoundary].cellType = GridCellType.RoomCorner;
            if(greaterGrid[leftBoundary, topBoundary].cellType != GridCellType.Door)
                greaterGrid[leftBoundary, topBoundary].cellType = GridCellType.RoomCorner;
            if(greaterGrid[rightBoundary, bottomBoundary].cellType != GridCellType.Door)
                greaterGrid[rightBoundary, bottomBoundary].cellType = GridCellType.RoomCorner;
            if(greaterGrid[rightBoundary, topBoundary].cellType != GridCellType.Door)
                greaterGrid[rightBoundary, topBoundary].cellType = GridCellType.RoomCorner;
        }
    }
    
    
    #endregion
    
    #region 生成角色和怪物的初始化位置

    public void GenerateCharacterAndMonsterInitPosition(int monsterDistFromPlayer, out Vector2Int characterInitPos, out Vector2Int monsterInitPos, out Vector2Int exitInitPos)
    {
        characterInitPos= Vector2Int.zero;
        monsterInitPos = Vector2Int.zero;
        exitInitPos = Vector2Int.zero;
        //选择最大连通分量中最左下的房间
        List<int> maxConnectedRooms = graph.FindMaxConnectedComponent();
        foreach (var room in maxConnectedRooms)
        {
            Debug.Log("房间 " + room + " 的位置是 " + rooms[room].basePosition + " 到 " + rooms[room].maxPosition);
        }
        if (maxConnectedRooms.Count <= 1)
        {
            Debug.LogError("没有找到最大的连通分量");
            return;
        }
        
        int minBaseposSum = int.MaxValue;
        int selectRoomIdx = -1;

        for (int i = 0; i < maxConnectedRooms.Count; i++)
        {
            var cur = maxConnectedRooms[i];
            var curBaseposSum = rooms[i].basePosition.x + rooms[i].basePosition.y;
            if (curBaseposSum < minBaseposSum)
            {
                minBaseposSum = curBaseposSum;
                selectRoomIdx = cur;
            }
        }

        characterInitPos.x = ((int)rooms[selectRoomIdx].center.x + offset) * 2;
        characterInitPos.y = ((int)rooms[selectRoomIdx].center.y + offset) * 2;

        var exitRoomIdx = graph.GetFarestNode(selectRoomIdx, out var maxDist);
        if (exitRoomIdx != -1)
        {
            exitInitPos.x = ((int)rooms[exitRoomIdx].center.x + offset) * 2;
            exitInitPos.y = ((int)rooms[exitRoomIdx].maxPosition.y - 1 + offset) * 2;
        }

        var monsterRoomIdx = graph.GetNodeWithGivenDistFromGivenRoom(selectRoomIdx, monsterDistFromPlayer);

        if (monsterRoomIdx != null && monsterRoomIdx.Count > 0)
        {
            //随机选择一个
            int randomIdx = Random.Range(0, monsterRoomIdx.Count);
            monsterInitPos.x = ((int)rooms[monsterRoomIdx[randomIdx]].center.x + offset) * 2;
            monsterInitPos.y = ((int)rooms[monsterRoomIdx[randomIdx]].center.y + offset) * 2; 
        }
        else
        {
            Debug.LogError("没有找到距离玩家" + monsterDistFromPlayer + "步的房间");
        }

    }

    public void InstantiatePlayer()
    {
        string playerPrefabPath = "Assets/Configs/Player/Player.prefab";
        #if UNITY_EDITOR
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
        #else
        var playerPrefab = Resources.Load<GameObject>(playerPrefabPath);
        #endif
        
        Debug.Log(playerBirthPos);
        GameObject player = GameObject.Instantiate(playerPrefab, new Vector3(playerBirthPos.x, 0, playerBirthPos.y), Quaternion.identity);

        string monsterPrefabPath = "Assets/Configs/Player/Monster.prefab";
        #if UNITY_EDITOR
        var monsterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(monsterPrefabPath);
        #else
        var monsterPrefab = Resources.Load<GameObject>(monsterPrefabPath);
        #endif
        
        Debug.Log(monsterBirthPos);
        GameObject monster = GameObject.Instantiate(monsterPrefab, new Vector3(monsterBirthPos.x, 1f, monsterBirthPos.y), Quaternion.identity);
        //monster下面的monsterChase的脚本需要绑定玩家的transform
        var monsterChase = monster.GetComponent<MonsterChase>();
        monsterChase.player = player.transform;
        monsterChase.dungeonMap = this;

        InstantiateGameManager();
    }

    public void InstantiateGameManager()
    {
        //实例化GameUI
        var gameUIPath = "Assets/Configs/UIPrefabs/GameUI.prefab";
        #if UNITY_EDITOR
        var gameUIPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gameUIPath);
        #else
        var gameUIPrefab = Resources.Load<GameObject>(gameUIPath);
        #endif
        
        GameObject gameUI = GameObject.Instantiate(gameUIPrefab);
        
        //实例化GameManager
        GameObject gameManager = new GameObject("GameManager");
        var manager = gameManager.AddComponent<GameManager>();
        manager.victoryUI = gameUI;
    }
    
    #endregion
    
    #region 根据三维坐标，提供距离为2的房间里的一个随机点，根据三维坐标找到greaterGrid的二维坐标

    public Vector3 GetRandomPointInRoom(Vector3 pos)
    {
        Vector2Int posInGreaterGrid = new Vector2Int((int)pos.x, (int)pos.z);
        var cellInfo = greaterGrid[posInGreaterGrid.x, posInGreaterGrid.y];
        Debug.Log("地板类型" + cellInfo.cellType);
        Debug.Log("房间ID" + cellInfo.roomID);
        if (cellInfo.roomID == -1)
        {
            Debug.LogError("是走廊！");
            return new Vector3(pos.x, 0, pos.z);
        }
        else
        {
            var room = rooms[cellInfo.roomID];
            Debug.Log("房间左下角" + room.basePosition);
            Debug.Log("房间size : " + room.size);
            var optionalRooms = graph.GetNodeWithGivenDistFromGivenRoom(cellInfo.roomID, 2);
            //从中找一个随机的房间
            if (optionalRooms != null && optionalRooms.Count > 0)
            {
                int randomIdx = Random.Range(0, optionalRooms.Count);
                var randomRoom = rooms[optionalRooms[randomIdx]];
                Debug.Log("传送房间左下角" + randomRoom.basePosition);
                Debug.Log("传送房间size : " + randomRoom.size);
                int roomCenterX = ((int)randomRoom.center.x + offset) * 2;
                int roomCenterY = ((int)randomRoom.center.y + offset) * 2;

                int roomMaxX = ((int)randomRoom.maxPosition.x - 1 + offset) * 2;
                int roomMaxY = ((int)randomRoom.maxPosition.y - 1 + offset) * 2;

                int roomBaseX = ((int)randomRoom.basePosition.x + offset) * 2;
                int roomBaseY = ((int)randomRoom.basePosition.y + offset) * 2;

                bool placeFounded = false;
                
                #region 从中心向四周遍历，找到floor
                for (int i = roomCenterX; i >= roomBaseX; i--)
                {
                    for (int j = roomCenterY; j >= roomBaseY; j--)
                    {
                        if (greaterGrid[i, j].cellType == GridCellType.Floor)
                            return new Vector3(i, 0, j);
                    }
                }
                for (int i = roomCenterX + 1; i <= roomMaxX; i++)
                {
                    for (int j = roomCenterY + 1; j <= roomMaxY; j++)
                    {
                        if (greaterGrid[i, j].cellType == GridCellType.Floor)
                            return new Vector3(i, 0, j);
                    }
                }
                for (int i = roomCenterX + 1; i <= roomMaxX; i++)
                {
                    for (int j = roomCenterY; j >= roomBaseY; j--)
                    {
                        if (greaterGrid[i, j].cellType == GridCellType.Floor)
                            return new Vector3(i, 0, j);
                    }
                }
                for (int i = roomCenterX; i >= roomBaseX; i--)
                {
                    for (int j = roomCenterY + 1; j <= roomMaxY; j++)
                    {
                        if (greaterGrid[i, j].cellType == GridCellType.Floor)
                            return new Vector3(i, 0, j);
                    }
                }
                #endregion
                
                Debug.LogError("在房间中找不到合适的位置");
                return new Vector3(pos.x, 0, pos.z);

            }
            else
            {
                Debug.LogError("没有找到距离为2的房间");
                return new Vector3(pos.x, 0, pos.z);
            }
        }
    }
    
    #endregion
}