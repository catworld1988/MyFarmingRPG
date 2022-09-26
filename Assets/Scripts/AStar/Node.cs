using System;
using UnityEngine;

public class Node : IComparable<Node>
{
    public Vector2Int gridPosition;
    public int gCost = 0; //到起点的代价
    public int hCost = 0; //到终点的代价
    public bool isObstacle = false; //障碍物
    public int movementPenalty; //移动惩罚
    public Node parentNode;


    //构造函数
    public Node(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;

        parentNode = null;
    }

    /// <summary>
    /// 代价之和
    /// </summary>
    public int FCost
    { get { return gCost + hCost; } }


    /// <summary>
    /// 比较代价
    /// </summary>
    public int CompareTo(Node nodeToCompare)
    {
        //比较之后<0 说明总代价 小于另外节点的总代价
        //比较之后>0 说明总代价 大于另外节点的总代价
        //比较之后=0 说明总代价 总代价相等

        int compare = FCost.CompareTo(nodeToCompare.FCost);
        //总代价相等,就再比较一下到 终点的代价
        if (compare==0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return compare;
    }
}
