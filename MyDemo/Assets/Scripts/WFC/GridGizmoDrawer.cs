using UnityEngine;

public class GridGizmoDrawer : MonoBehaviour
{
    [Header("网格参数")]
    public int gridSize = 10;      // 网格总大小（0~100）
    public float cellSize = 1f;     // 每个cell的大小
    public Color gridColor = Color.green;

    public WFCMain wfc; // 拖拽引用
    
    private void OnDrawGizmos()
    {
        Gizmos.color = gridColor;
        
        // 画网格线
        for (int x = 0; x <= gridSize; x++)
        {
            Vector3 start = new Vector3(x * cellSize, 0, 0);
            Vector3 end = new Vector3(x * cellSize, 0, gridSize * cellSize);
            Gizmos.DrawLine(start, end);
        }
        for (int z = 0; z <= gridSize; z++)
        {
            Vector3 start = new Vector3(0, 0, z * cellSize);
            Vector3 end = new Vector3(gridSize * cellSize, 0, z * cellSize);
            Gizmos.DrawLine(start, end);
        }

        // 显示WFC结果
        if (wfc != null)
        {
            for (int i = 0; i < wfc.width; i++)
            {
                for (int j = 0; j < wfc.height; j++)
                {
                    Vector3 center = new Vector3(i * cellSize + cellSize/2, 0, j * cellSize + cellSize/2);
                    if (wfc.slots[i,j].collapsed)
                    {
                        Gizmos.color = wfc.slots[i,j].module == SlotType.Blank ? Color.white : 
                                      wfc.slots[i,j].module == SlotType.BookShelfTop ? Color.red : Color.blue;
                        
                        Gizmos.DrawCube(center, new Vector3(cellSize*0.9f, 0.1f, cellSize*0.9f));
                    }
                }
            }
        }
    }
}