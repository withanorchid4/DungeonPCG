using UnityEngine;

public class TestGraph : MonoBehaviour
{
    private MatrixGraph graph;

    void Start()
    {
        // 初始化一个5个节点的图
        graph = new MatrixGraph(6);

        // 测试1：添加边并验证
        TestAddEdge();

        // 测试2：计算最短距离并验证
        TestBuildDistances();

        // 测试3：查找最远节点并验证
        TestGetFarestNode();

        // 测试4：查找指定距离的节点并验证
        TestGetNodeWithGivenDistFromGivenRoom();

        // 测试5：检查节点是否孤立并验证
        TestIsDisconnected();

        // 测试6：查找最大连通分量并验证
        TestFindMaxConnectedComponent();
    }

    private void TestAddEdge()
    {
        graph.AddEdge(0, 1, 2);
        graph.AddEdge(1, 2, 3);
        graph.AddEdge(2, 3, 1);
        graph.AddEdge(3, 4, 4);

        Debug.Log("TestAddEdge: 边添加完成");
    }

    private void TestBuildDistances()
    {
        graph.BuildDistances();
        Debug.Log("TestBuildDistances: 最短距离计算完成");
    }

    private void TestGetFarestNode()
    {
        int maxDistance;
        int farNode = graph.GetFarestNode(0, out maxDistance);
        Debug.Log($"TestGetFarestNode: 距离节点0最远的节点是{farNode}，距离为{maxDistance}");
    }

    private void TestGetNodeWithGivenDistFromGivenRoom()
    {
        var nodes = graph.GetNodeWithGivenDistFromGivenRoom(2, 2);
        Debug.Log("TestGetNodeWithGivenDistFromGivenRoom: 距离节点0为2的节点有：" + string.Join(", ", nodes));
    }

    private void TestIsDisconnected()
    {
        bool isDisconnected = graph.IsDisconnected(5);
        Debug.Log($"TestIsDisconnected: 节点4是否孤立？{isDisconnected}");
    }

    private void TestFindMaxConnectedComponent()
    {
        var maxComponent = graph.FindMaxConnectedComponent();
        Debug.Log("TestFindMaxConnectedComponent: 最大连通分量的节点有：" + string.Join(", ", maxComponent));
    }
}
