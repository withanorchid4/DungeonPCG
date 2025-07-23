using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

public class ObjectPlacementSystem
{
#if UNITY_EDITOR
    private static PrefabResource prefabResource = AssetDatabase.LoadAssetAtPath<PrefabResource>("Assets/Configs/PrefabBind.asset");
    private static PrefabResource alongWallResource = AssetDatabase.LoadAssetAtPath<PrefabResource>("Assets/Configs/DecorPrefabs/AlongWallPrefabs/AlongWallPrefab.asset");
#else
    private static prefabResource = Resources.Load<PrefabResource>("Assets/ArtForSecondScene/ZNS3D/Prefabs/phone.prefab");
#endif
    public static void PlaceObject(Vector3 position, string objName)
    {
        if (prefabResource == null)
        {
            Debug.LogError("Error: 找不到prefabBind资产");
        }

        for (uint i = 0; i < prefabResource.prefabInfos.Length; i++)
        {
            if (prefabResource.prefabInfos[i].prefab.name.Contains(objName))
            {
                var prefab = prefabResource.prefabInfos[i].prefab;
                GameObject go = Object.Instantiate(prefab, position, quaternion.identity);
                go.name = objName;
            }
        }
    }
    
    public static void PlaceDoor(DoorData data, float x, float y, string doorType, CellInfo[,] grid = null)
    {
        if (prefabResource == null)
        {
            Debug.LogError("Error: 找不到prefabBind资产");
        }

        bool found = false;
        for (uint i = 0; i < prefabResource.prefabInfos.Length; i++)
        {
            if (prefabResource.prefabInfos[i].prefab.name.Contains(doorType))
            {
                found = true;
                var prefabInfo = prefabResource.prefabInfos[i];

                float angle = data.GetRotationAngle();
                if (grid == null)
                {
                    GameObject tempGo = Object.Instantiate(prefabInfo.prefab, new Vector3(x, 0, y), Quaternion.Euler(0, angle, 0));
                    tempGo.name = "Temp" + doorType + data.doorID;
                    //tempGo.AddComponent<DoorControl>();
                    return;
                }
                
                GameObject go = Object.Instantiate(prefabInfo.prefab, new Vector3(x, 0, y), Quaternion.Euler(0, angle, 0));
                go.name = doorType + data.doorID;
                if (data.isExit)
                {
                    go.name = go.name + "_Exit";
                }
                //go.AddComponent<DoorControl>();
            }
        }
        //Debug.Log("Door founded ? " + found);
    }
    public static void PlaceTreasureBox(float x, float y, CellInfo[,] greaterGrid = null)
    {
        if (prefabResource == null)
        {
            Debug.LogError("Error: 找不到prefabBind资产");
        }

        for (uint i = 0; i < prefabResource.prefabInfos.Length; i++)
        {
            if (prefabResource.prefabInfos[i].prefab.name.Contains("TreasureBox"))
            {
                var prefabInfo = prefabResource.prefabInfos[i];
                
                //For Debug，在示意图中实例化，直接在目标位置生成，放缩和地牢中不一样
                if (greaterGrid == null)
                {
                    GameObject tempGo = Object.Instantiate(prefabInfo.prefab, new Vector3(x, 0, y), Quaternion.identity);
                    tempGo.name = "TempTreasureBox";
                    return;
                }
                //Debug end
                
                if (!CouldPlacedHere(x, y, prefabInfo, greaterGrid))
                {
                    return;
                }

                GameObject go = Object.Instantiate(prefabInfo.prefab, new Vector3(x, 0, y), Quaternion.identity);
                go.name = "TreasureBox";

            }
        }
        
    }

    public static void PlaceCenterAreaObject(float centerX, float centerY, string objName, CellInfo[,] greaterGrid = null)
    {
        if (prefabResource == null)
        {
            Debug.LogError("Error: 找不到prefabBind资产");
        }

        for (uint i = 0; i < prefabResource.prefabInfos.Length; i++)
        {
            Debug.Log(prefabResource.prefabInfos[i].prefab.name);
            if (prefabResource.prefabInfos[i].prefab.name.Contains(objName))
            {
                Debug.Log(prefabResource.prefabInfos[i].prefab.name);
                var prefabInfo = prefabResource.prefabInfos[i];

                var baseX = centerX - prefabInfo.size.x / 2;
                var baseY = centerY - prefabInfo.size.z / 2;
                //For Debug，在示意图中实例化，直接在目标位置生成，放缩和地牢中不一样
                if (greaterGrid == null)
                {
                    GameObject tempGo = Object.Instantiate(prefabInfo.prefab, new Vector3(baseX, 0.02f, baseY), Quaternion.identity);
                    tempGo.name = "Temp" + prefabInfo.prefab.name;
                    continue;
                }
                //Debug end
                
                if (!CouldPlacedHere(baseX, baseY, prefabInfo, greaterGrid))
                {
                    return;
                }

                GameObject go = Object.Instantiate(prefabInfo.prefab, new Vector3(baseX, 0.02f, baseY), Quaternion.identity);
                //go.name += centerX;
            }
        }
        
    }
    private static bool CouldPlacedHere(float x, float y, PrefabInfo prefabInfo, CellInfo[,] grid)
    {
        var decorType = prefabInfo.type;
        if (decorType == PrefabType.OnFloor)
        {
            return true;
        }
        else if(decorType == PrefabType.ObstacleOnFloor) //门不走这个逻辑
        {
            for (int m = (int)(x + prefabInfo.basePosition.x);
                 m < x + prefabInfo.basePosition.x + prefabInfo.size.x;
                 m++)
            {
                for (int n = (int)(y + prefabInfo.basePosition.z);
                     n < y + prefabInfo.basePosition.z + prefabInfo.size.z;
                     n++)
                {
                    if (grid[m, n].cellType == GridCellType.Occupied || grid[m, n].cellType == GridCellType.None)
                    {
                        Debug.Log("在(" + m + ", " + n + ")处有占用，无法放置" + prefabInfo.prefab.name);
                        return false;
                    }
                }
            }
            
            for (int m = (int)(x + prefabInfo.basePosition.x);
                 m < x + prefabInfo.basePosition.x + prefabInfo.size.x;
                 m++)
            {
                for (int n = (int)(y + prefabInfo.basePosition.z);
                     n < y + prefabInfo.basePosition.z + prefabInfo.size.z;
                     n++)
                {
                    grid[m, n].cellType = GridCellType.Occupied;
                }
            }

            return true;
        }
        else
        {
            Debug.Log("出现未定义的prefabType，无法处理");
            return true;
        }
    }

    public static bool CouldWallObjectPlacedHere(float x, float y, int wallType, string objName, CellInfo[,] greaterGrid)  //沿墙物体都是有方向性的，不能走上面的函数逻辑
    {
        if (alongWallResource == null)
        {
            Debug.LogError("Error: 找不到alongWallPrefabBind资产");
            return false;
        }

        for (uint i = 0; i < alongWallResource.prefabInfos.Length; i++)
        {
            if (alongWallResource.prefabInfos[i].prefab.name.Contains(objName))
            {
                var prefabInfo = alongWallResource.prefabInfos[i];
                if (wallType == 0)
                {
                    for (int m = (int)(x + prefabInfo.basePosition.x); m < x + prefabInfo.basePosition.x + prefabInfo.size.x; m++)
                    {
                        for (int n = (int)(y + prefabInfo.basePosition.z);
                             n < y + prefabInfo.basePosition.z + prefabInfo.size.z;
                             n++)
                        {
                            if (greaterGrid[m, n].cellType == GridCellType.Occupied ||
                                greaterGrid[m, n].cellType == GridCellType.None ||
                                greaterGrid[m, n].cellType == GridCellType.Door ||
                                greaterGrid[m, n].cellType == GridCellType.Exit ||
                                greaterGrid[m, n].cellType == GridCellType.CorridorOnFloor) //希望靠墙的物体也不要占用过道
                            {
                                Debug.Log("在(" + m + ", " + n + ")处有占用，无法放置" + prefabInfo.prefab.name);
                                return false;
                            }
                        }
                    }
                    
                    for (int m = (int)(x + prefabInfo.basePosition.x); m < x + prefabInfo.basePosition.x + prefabInfo.size.x; m++)
                    {
                        for (int n = (int)(y + prefabInfo.basePosition.z);
                             n < y + prefabInfo.basePosition.z + prefabInfo.size.z;
                             n++)
                        {
                            greaterGrid[m, n].cellType = GridCellType.Occupied;
                        }
                    }

                    //在此处实例化
                    GameObject leftAlongWallObj = Object.Instantiate(prefabInfo.prefab,
                        new Vector3(x + prefabInfo.size.x / 2, 0, y + prefabInfo.size.z / 2 - 1), Quaternion.identity);
                    leftAlongWallObj.name = "LeftBookShelf";
                }
                else if (wallType == 1)
                {
                    for (int m = (int)(x + prefabInfo.basePosition.z);
                         m < x + prefabInfo.basePosition.z + prefabInfo.size.z;
                         m++)
                    {
                        for (int n = (int)(y + prefabInfo.basePosition.x); n < y + prefabInfo.basePosition.x +
                             prefabInfo.size.x; n++)
                        {
                            if (greaterGrid[m, n].cellType == GridCellType.Occupied ||
                                greaterGrid[m, n].cellType == GridCellType.None ||
                                greaterGrid[m, n].cellType == GridCellType.Door ||
                                greaterGrid[m, n].cellType == GridCellType.Exit ||
                                greaterGrid[m, n].cellType == GridCellType.CorridorOnFloor) //希望靠墙的物体也不要占用过道
                            {
                                Debug.Log("在(" + m + ", " + n + ")处有占用，无法放置" + prefabInfo.prefab.name);
                                return false;
                            }
                        }
                    }
                    for (int m = (int)(x + prefabInfo.basePosition.z);
                         m < x + prefabInfo.basePosition.z + prefabInfo.size.z;
                         m++)
                    {
                        for (int n = (int)(y + prefabInfo.basePosition.x); n < y + prefabInfo.basePosition.x +
                             prefabInfo.size.x; n++)
                        {
                            greaterGrid[m, n].cellType = GridCellType.Occupied;
                        }
                    }
                    GameObject buttomAlongWallObj = Object.Instantiate(prefabInfo.prefab,
                        new Vector3(x + prefabInfo.size.z / 2, 0, y + prefabInfo.size.x / 2 - 1), Quaternion.Euler(0,270,0));
                    buttomAlongWallObj.name = "ButtomBookShelf";
                }
                else if (wallType == 2)
                {
                    for (int m = (int)(x + prefabInfo.basePosition.x);
                         m > x + prefabInfo.basePosition.x - prefabInfo.size.x;
                         m--)
                    {
                        for (int n = (int)(y + prefabInfo.basePosition.z);
                             n < y + prefabInfo.basePosition.z + prefabInfo.size.z;
                             n++)
                        {
                            if (greaterGrid[m, n].cellType == GridCellType.Occupied ||
                                greaterGrid[m, n].cellType == GridCellType.None ||
                                greaterGrid[m, n].cellType == GridCellType.Door ||
                                greaterGrid[m, n].cellType == GridCellType.Exit ||
                                greaterGrid[m, n].cellType == GridCellType.CorridorOnFloor) //希望靠墙的物体也不要占用过道
                            {
                                Debug.Log("在(" + m + ", " + n + ")处有占用，无法放置" + prefabInfo.prefab.name);
                                return false;
                            }
                        }
                    }
                    for (int m = (int)(x + prefabInfo.basePosition.x);
                         m > x + prefabInfo.basePosition.x - prefabInfo.size.x;
                         m--)
                    {
                        for (int n = (int)(y + prefabInfo.basePosition.z);
                             n < y + prefabInfo.basePosition.z + prefabInfo.size.z;
                             n++)
                        {
                            greaterGrid[m, n].cellType = GridCellType.Occupied;
                        }
                    }
                    GameObject rightAlongWallObj = Object.Instantiate(prefabInfo.prefab,
                        new Vector3(x - prefabInfo.size.x / 2 + 1, 0, y + prefabInfo.size.z / 2 - 1), Quaternion.Euler(0,180,0));
                    rightAlongWallObj.name = "RightBookShelf";
                }
                else
                {
                    for (int m = (int)(x + prefabInfo.basePosition.z);
                         m < x + prefabInfo.basePosition.z + prefabInfo.size.z;
                         m++)
                    {
                        for (int n = (int)(y + prefabInfo.basePosition.x);
                             n > y + prefabInfo.basePosition.x - prefabInfo.size.x;
                             n--)
                        {
                            if (greaterGrid[m, n].cellType == GridCellType.Occupied ||
                                greaterGrid[m, n].cellType == GridCellType.None ||
                                greaterGrid[m, n].cellType == GridCellType.Door ||
                                greaterGrid[m, n].cellType == GridCellType.Exit ||
                                greaterGrid[m, n].cellType == GridCellType.CorridorOnFloor) //希望靠墙的物体也不要占用过道
                            {
                                Debug.Log("在(" + m + ", " + n + ")处有占用，无法放置" + prefabInfo.prefab.name);
                                return false;
                            }
                        }
                    }
                    for (int m = (int)(x + prefabInfo.basePosition.z);
                         m < x + prefabInfo.basePosition.z + prefabInfo.size.z;
                         m++)
                    {
                        for (int n = (int)(y + prefabInfo.basePosition.x);
                             n > y + prefabInfo.basePosition.x - prefabInfo.size.x;
                             n--)
                        {
                            greaterGrid[m, n].cellType = GridCellType.Occupied;
                            Debug.Log("在(" + m + ", " + n + ")处放置" + prefabInfo.prefab.name);
                        }
                    }
                    GameObject topAlongWallObj = Object.Instantiate(prefabInfo.prefab,
                        new Vector3(x + prefabInfo.size.z / 2, 0, y - prefabInfo.size.x / 2), Quaternion.Euler(0,90,0));
                    topAlongWallObj.name = "TopBookShelf";
                }
            }
        }
        return true;
    }
}
