using System.Collections.Generic;

using UnityEngine;

public struct Edge
{
    public int from;
    public int to;
    public int weight;
    public Edge(int f, int t, int w)
    {
        from = f;
        to = t;
        weight = w;
    }
}


public class MatrixGraph
{
    public int[,] matrix;

    public int[,] distanceMatrix; // 新增：存储所有节点对的最短距离

    private int[,] nodeDistMatrix;
    
    public MatrixGraph(int length)
    {
        matrix = new int[length, length];
        distanceMatrix = new int[length, length];
        nodeDistMatrix = new int[length, length];
        InitializeDistanceMatrix();
    }
    
    private void InitializeDistanceMatrix() {
        int n = matrix.GetLength(0);
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                // 对角线为0，其他初始化为无穷大
                matrix[i, j] = (i == j) ? 0 : int.MaxValue;
                distanceMatrix[i, j] = (i == j) ? 0 : int.MaxValue;
                nodeDistMatrix[i, j] = (i == j) ? 0 : int.MaxValue;
            }
        }
    }
    
    //添加边
    public void AddEdge(int row, int col, int value)
    {
        matrix[row, col] = value;
        matrix[col, row] = value;
    }

    //生成最小生成树MST
    public List<Edge> BuildMST(int start)
    {
        int n = matrix.GetLength(0);
        bool[] visited = new bool[n];
        int[] minEdge = new int[n]; // 到MST的最小权重
        int[] parent = new int[n];  // 记录最小权重边的父节点
        List<Edge> mstEdges = new List<Edge>();

        for (int i = 0; i < n; i++)
        {
            minEdge[i] = int.MaxValue;
            parent[i] = -1;
        }
        minEdge[start] = 0; // 从0号节点开始，图必然连通

        for (int i = 0; i < n; i++)
        {
            int u = -1;
            for (int v = 0; v < n; v++)
            {
                if (!visited[v] && (u == -1 || minEdge[v] < minEdge[u]))
                    u = v;
            }
            if (minEdge[u] == int.MaxValue)
                break; // 剩下的点不连通

            visited[u] = true;

            if (parent[u] != -1)
            {
                mstEdges.Add(new Edge(parent[u], u, matrix[parent[u], u]));
            }

            for (int v = 0; v < n; v++)
            {
                if (matrix[u, v] != 0 && !visited[v] && matrix[u, v] < minEdge[v])
                {
                    minEdge[v] = matrix[u, v];
                    parent[v] = u;
                }
            }
        }

        return mstEdges;
    }
    public void BuildDistances() //添加完走廊之后就可以生成最短距离矩阵
    {
        int n = matrix.GetLength(0);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (matrix[i, j] != 0)
                {
                    distanceMatrix[i, j] = matrix[i, j];
                    nodeDistMatrix[i, j] = matrix[i, j] == int.MaxValue ? int.MaxValue : 1;
                }
            }
        }

        for (int k = 0; k < n; k++)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (distanceMatrix[i, k] != int.MaxValue && distanceMatrix[k, j] != int.MaxValue)
                    {
                        int newDist = distanceMatrix[i, k] + distanceMatrix[k, j];
                        if (newDist < distanceMatrix[i, j])
                        {
                            distanceMatrix[i, j] = newDist;
                        }
                        
                        int newNodeDist = nodeDistMatrix[i, k] + nodeDistMatrix[k, j];
                        if (newNodeDist < nodeDistMatrix[i, j])
                        {
                            nodeDistMatrix[i, j] = newNodeDist;
                        }
                    }
                }
            }
        }
    }

    public int GetFarestNode(int index, out int maxDistance)
    {
        maxDistance = 0;
        int maxIndex = -1;
        for (int i = 0; i < distanceMatrix.GetLength(0); i++)
        {
            if (distanceMatrix[index, i] > maxDistance && i != index && distanceMatrix[index, i] != int.MaxValue)
            {
                maxDistance = distanceMatrix[index, i];
                maxIndex = i;
            }
        }

        return maxIndex;
    }

    public List<int> FindWayFromStartToEnd(int start, int end)  //BFS实现
    {
        List<int> path = new List<int>();
        if (start < 0 || start >= distanceMatrix.GetLength(0) || end < 0 || end >= distanceMatrix.GetLength(0))
        {
            Debug.Log("[FindWayFromStartToEnd] 参数错误");
            return path;
        }
        
        List<int>[] pathList = new List<int>[distanceMatrix.GetLength(0)];
        bool[] visited = new bool[distanceMatrix.GetLength(0)];

        for (int i = 0; i < distanceMatrix.GetLength(0); i++)
        {
            pathList[i] = new List<int>();
            visited[i] = false;
        }
        pathList[start].Add(start);
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            visited[current] = true;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                if (matrix[current, i] != int.MaxValue && !visited[i])
                {
                    pathList[i].AddRange(pathList[current]);
                    pathList[i].Add(i);
                    queue.Enqueue(i);
                }
            }
        }

        return pathList[end];
    }

    public List<int> GetNodeWithGivenDistFromGivenRoom(int index, int dist)
    {
        if (index < 0 || index >= distanceMatrix.GetLength(0) || dist < 0)
        {
            Debug.Log("[GetNodeWithGivenDistFromGivenRoom] 参数错误");
            return null;
        }
        var nodeList = new List<int>();
        for (int i = 0; i < nodeDistMatrix.GetLength(0); i++)
        {
            if (nodeDistMatrix[index, i] == dist)
            {
                nodeList.Add(i);
            }
        }

        return nodeList;
    }
    
    //判断当前房间是否和其他房间都不联通
    public bool IsDisconnected(int index)
    {
        for (int i = 0; i < distanceMatrix.GetLength(0); i++)
        {
            if (i != index && distanceMatrix[index, i] != int.MaxValue)
            {
                return false;
            }
        }
        return true;
    }
    
    //此图可能存在多个连通分量，找到其中最大的一个连通分量，返回这个分量的节点列表
    public List<int> FindMaxConnectedComponent()
    {
        List<int> connectedComponents = new List<int>();
        int selectIdx = -1;
        int maxSize = 1;
        for (int i = 0; i < distanceMatrix.GetLength(0); i++)
        {
            int connectedSize = 0;
            for (int j = 0; j < distanceMatrix.GetLength(0); j++)
            {
                if (distanceMatrix[i, j] != int.MaxValue)
                {
                    connectedSize++;
                }
            }

            if (connectedSize > maxSize)
            {
                maxSize = connectedSize;
                selectIdx = i;
            }
        }

        if (selectIdx != -1)
        {
            for (int i = 0; i < distanceMatrix.GetLength(0); i++)
            {
                if (distanceMatrix[selectIdx, i] != int.MaxValue)
                {
                    connectedComponents.Add(i);
                }
            }
        }

        return connectedComponents;
    }
    
    //返回连通分量
    public List<List<int>> GetConnectedComponents()
    {
        List<List<int>> connectedComponents = new List<List<int>>();
        
        bool []visited = new bool[distanceMatrix.GetLength(0)];
        for (int i = 0; i < distanceMatrix.GetLength(0); i++)
        {
            visited[i] = false;
        }
        
        for (int i = 0; i < distanceMatrix.GetLength(0); i++)
        {
            if (!visited[i])
            {
                visited[i] = true;
                List<int> component = new List<int>();
                component.Add(i);
                for (int j = 0; j < distanceMatrix.GetLength(0); j++)
                {
                    if (j != i && distanceMatrix[i, j] != int.MaxValue && !visited[j])
                    {
                        component.Add(j);
                        visited[j] = true;
                    }
                }
                connectedComponents.Add(component);
            }
        }
        return connectedComponents;
    }
}
