using System.Collections.Generic;

using UnityEngine;

public class MatrixGraph
{
    public int[,] matrix;

    private int[,] distanceMatrix; // 新增：存储所有节点对的最短距离

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
                    nodeDistMatrix[i, j] = 1;
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
}
