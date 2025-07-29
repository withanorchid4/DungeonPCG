using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFCMain
{
    public int height;
    public int width;
    public Slot[,] slots;

    private int hasCollapsedCount;
    
    // 新增：历史记录栈（保存状态和可回溯的决策点）
    Vector2Int lastCollapsePos;
    private Stack<(Slot[,] slots, int hasCollapsedCount, Vector2Int lastCollapsePos, List<SlotType> triedModules)> historyStack = new Stack<(Slot[,], int, Vector2Int, List<SlotType>)>();
    
    // 新增：当前决策点的剩余可选模块缓存
    private Dictionary<Vector2Int, List<SlotType>> remainingChoices = new Dictionary<Vector2Int, List<SlotType>>();
    
    public WFCMain(int height, int width)
    {
        this.height = height;
        this.width = width;
        slots = new Slot[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                slots[i, j] = new Slot() 
                {
                    collapsed = false,
                    modules = new List<SlotType>(){SlotType.BookShelfBottom, SlotType.Blank, SlotType.BookShelfTop}
                };
            }
        }

        
    }

    public Slot[,] BackupSlots()
    {
        var newSlots = new Slot[width, height];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                newSlots[i, j] = this.slots[i, j].Backup();
            }
        }

        return newSlots;
    }
    public void InitializeRandomly()
    {
        // slots[height/2, width/2].collapsed = true;
        // slots[height / 2, width / 2].module = SlotType.BookShelfBottom;
        // hasCollapsedCount = 1;
        // Propagate(new Vector2Int(height / 2, width / 2));
        slots[0, 0].modules.Remove(SlotType.Blank);
        slots[0, 0].modules.Remove(SlotType.BookShelfTop);

        slots[1, 0].modules.Remove(SlotType.Blank);
        slots[1, 0].modules.Remove(SlotType.BookShelfBottom);
    }

    public void DoMagic()
    {
        while (hasCollapsedCount < height * width)
        {
            var backup = BackupSlots();
            historyStack.Push((backup, hasCollapsedCount, lastCollapsePos, remainingChoices.Values.SelectMany(x => x).ToList()));
            
            var minCoords = GetMinEntropyCorrds();
            lastCollapsePos = minCoords;
            
            //尝试坍缩
            bool collapseSuccess = CollapseAt(minCoords);
            if (!collapseSuccess)
            {
                HandleBacktrack();
                continue;
            }

            bool propagateSuccess = Propagate(minCoords);
            if (!propagateSuccess)
            {
                HandleBacktrack();
            }
            
            //Iterate();
        }
    }
    
    public void Iterate()
    {
        var minEntropyCorrds = GetMinEntropyCorrds();
        CollapseAt(minEntropyCorrds);
        Propagate(minEntropyCorrds);
    }

    private bool CollapseAt(Vector2Int corrds, bool isBackTracking = false)
    {
        var slot = slots[corrds.x, corrds.y];
        if (slot.modules.Count == 0)
        {
            Debug.LogError("矛盾发生，需要回溯");
            return false;
        }

        if (isBackTracking && remainingChoices.ContainsKey(corrds))
        {
            var remaining = remainingChoices[corrds];
            if (remaining.Count > 0)
            {
                slot.module = remaining[0];
                remaining.RemoveAt(0);
            }
            else
            {
                return false;
            }
        }
        else
        {
            remainingChoices[corrds] = new List<SlotType>(slot.modules);
            
            //找一个随机的模块——>按照一定的权重来选择，优先添加blank
            var weightedList = new List<SlotType>();
            foreach (var m in slots[corrds.x, corrds.y].modules)
            {
                for (int i = 0; i < NeighborConstrainRule.slotTypeWeight[m]; i++)
                {
                    weightedList.Add(m);
                }
            }
            var module = weightedList[Random.Range(0, weightedList.Count)];
            slot.module = module;
            // slots[corrds.x, corrds.y].module = module;
            // slots[corrds.x, corrds.y].collapsed = true;
            // slots[corrds.x, corrds.y].modules.Clear();
            // slots[corrds.x, corrds.y].modules.Add(module);
            // hasCollapsedCount++;
        }
        
        slot.collapsed = true;
        slot.modules.Clear();
        slot.modules.Add(slot.module);
        hasCollapsedCount++;

        return true;
    }

    private bool Propagate(Vector2Int corrds)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(corrds);
        while (stack.Count > 0)
        {
            var corrd = stack.Pop();
            var curModules = slots[corrd.x, corrd.y].modules;
            
            var validDirs = GetValidDirs(corrd);
            foreach (var dirCorrd in validDirs)
            {
                var dir = dirCorrd.Item1;
                var neighborCorrd = dirCorrd.Item2;
                
                var neighborModules = slots[neighborCorrd.x, neighborCorrd.y].modules;

                var shouldNeighborModules = new HashSet<SlotType>();

                foreach (var module in curModules)
                {
                    var neightborMod = NeighborConstrainRule.GetNeightborSlots(module, dir);
                    shouldNeighborModules.UnionWith(neightborMod);
                }
                

                // 先收集需要移除的模块
                var toRemove = new List<SlotType>();
                foreach (var module in neighborModules)
                {
                    if (!shouldNeighborModules.Contains(module))
                    {
                        toRemove.Add(module);
                    }
                }

                if (toRemove.Count == neighborModules.Count)
                {
                    Debug.Log($"矛盾发生在 {neighborCorrd}");
                    return false;
                }
                // 再统一移除
                foreach (var module in toRemove)
                {
                    neighborModules.Remove(module);
                    if(!stack.Contains(neighborCorrd))
                        stack.Push(neighborCorrd);
                }
            }
        }

        return true;
    }

    private List<(int, Vector2Int)> GetValidDirs(Vector2Int corrds)
    {
        List<(int, Vector2Int)> dirs = new List<(int, Vector2Int)>();
        if (corrds.x > 0 && !slots[corrds.x - 1, corrds.y].collapsed)
        {
            dirs.Add((0, new Vector2Int(corrds.x - 1, corrds.y)));
        }

        if (corrds.x < width - 1 && !slots[corrds.x + 1, corrds.y].collapsed)
        {
            dirs.Add((2, new Vector2Int(corrds.x + 1, corrds.y)));
        }

        if (corrds.y > 0 && !slots[corrds.x, corrds.y - 1].collapsed)
        {
            dirs.Add((1, new Vector2Int(corrds.x, corrds.y - 1)));
        }

        if (corrds.y < height - 1 && !slots[corrds.x, corrds.y + 1].collapsed)
        {
            dirs.Add((3, new Vector2Int(corrds.x, corrds.y + 1)));
        }
        
        //额外增加四个方向——先不做
        
        return dirs;
    }
    public Vector2Int GetMinEntropyCorrds()
    {
        Vector2Int minEntropyCorrds = Vector2Int.zero;
        float minEntropy = float.MaxValue;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (!slots[i, j].collapsed)
                {
                    float entropy = slots[i, j].modules.Count;
                    if (entropy < minEntropy)
                    {
                        minEntropy = entropy;
                        minEntropyCorrds.x = i;
                        minEntropyCorrds.y = j;
                    }
                }
            }
        }

        return minEntropyCorrds;
    }
    
    private void HandleBacktrack()
    {
        if (historyStack.Count == 0)
        {
            Debug.LogError("无法回溯，无解");
            throw new System.Exception("No solution");
        }

        Debug.LogError("触发回溯！！");
        // 恢复上一个状态
        var (backupSlots, backupCount, lastPos, remaining) = historyStack.Pop();
        slots = backupSlots;
        hasCollapsedCount = backupCount;
        remainingChoices[lastPos] = remaining;

        // 重新尝试坍缩（强制进入回溯模式）
        bool retrySuccess = CollapseAt(lastPos, true);
        if (!retrySuccess)
        {
            HandleBacktrack(); // 递归回溯
        }
        else
        {
            // 重新传播
            bool propagateSuccess = Propagate(lastPos);
            if (!propagateSuccess)
            {
                HandleBacktrack();
            }
        }
    }
    
    
}
