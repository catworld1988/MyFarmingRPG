using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    //是否观察处罚
    [Header("Tiles & Tilemap References")]
    [Header("Options")]
    [SerializeField] private bool observeMovementPenalties = true;

    //移动路径处罚
    [Range(0, 20)]
    [SerializeField] private int pathMovementPenalty = 0;
    [Range(0, 20)]
    [SerializeField] private int defaultMovementPenalty = 0;

    private GridNodes gridNodes;
    private Node startNode;
    private Node targetNode;
    private int gridWidth;
    private int gridHeight;
    private int originX;
    private int originY;

    private List<Node> openNodesList;
    //弃用节点的封闭列表
    private HashSet<Node> closedNodeList;

    private bool pathFound = false;


    /// <summary>
    /// Builds a path for the given sceneName from the startGridPosition to the endGridPosition, and adds movement steps to the passed in npcMovementstack.
    /// Also returns true if path found or false if no path found. 从传入的场景名 建立个从起点到终点的路径,并且添加移动步数到npc的堆栈中.也返回是否找到路径
    /// </summary>
    public bool BuildPath(SceneName sceneName, Vector2Int startGridPosition, Vector2Int endGridPosition, Stack<NPCMovementStep> npcMovementStepStack)
    {
        pathFound = false;

        if (populateGridNodesFromGridPropertiesDictionary(sceneName,startGridPosition,endGridPosition))
        {
            if (FindShortesPath())
            {
                UpdatePathOnNPCMovementStepStack(sceneName, npcMovementStepStack);

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 用堆栈更新NPC移动
    /// </summary>
    private void UpdatePathOnNPCMovementStepStack(SceneName sceneName, Stack<NPCMovementStep> npcMovementStepStack)
    {
        Node nextNode = targetNode;

        while (nextNode!=null)
        {
            NPCMovementStep npcMovementStep = new NPCMovementStep();
            npcMovementStep.sceneName = sceneName;
            npcMovementStep.gridCoordinate = new Vector2Int(nextNode.gridPosition.x + originX, nextNode.gridPosition.y + originY);

            //最后一个是你取出的第一个  放入堆栈的最后一个是开始节点
            npcMovementStepStack.Push(npcMovementStep);

            //开始节点没有父级节点 跳出循环
            nextNode = nextNode.parentNode;
        }
    }

    /// <summary>
    /// 寻找最短路径 找到返回true
    /// </summary>
    private bool FindShortesPath()
    {
        //添加开始节点到 开放列表
        openNodesList.Add(startNode);

        //遍历开放列表直到空
        while (openNodesList.Count>0)
        {
            //排序列表
            openNodesList.Sort();

            //找到currentNode代价最低的节点从 开放列表移除
            Node currentNode = openNodesList[0];
            openNodesList.RemoveAt(0);

            //添加currentNode到 封闭列表
            closedNodeList.Add(currentNode);

            //如果currentNode是 目标节点 结束寻路
            if (currentNode== targetNode)
            {
                pathFound = true;
                break;
            }

            //评估当前节点的邻居节点
            EvaluateCurrentNodeNeighbours(currentNode);
        }

        if (pathFound)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 评估8个邻居节点的代价
    /// </summary>
    private void EvaluateCurrentNodeNeighbours(Node currentNode)
    {
        //当前节点的位置
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;
        Node validNeighbourNode;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <=1; j++)
            {
                //如果遍历到当前节点则略过 继续
                if (i==0 && j==0)
                    continue;
                ////邻居节点的有效性
                validNeighbourNode = GetValidNodeNeighbour(currentNodeGridPosition.x + i, currentNodeGridPosition.y + j);

                if (validNeighbourNode!=null)
                {
                    //计算邻居节点的代价
                    int newCostToNeighbour;
                    //如果启用惩罚
                    if (observeMovementPenalties)
                    {
                        newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode) + validNeighbourNode.movementPenalty;
                    }
                    else
                    {
                        newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode);
                    }

                    bool isValidNeighbourNodeInOpenList = openNodesList.Contains(validNeighbourNode);

                    if (newCostToNeighbour< validNeighbourNode.gCost || !isValidNeighbourNodeInOpenList)
                    {
                        validNeighbourNode.gCost = newCostToNeighbour;
                        //曼哈顿距离
                        validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);

                        validNeighbourNode.parentNode = currentNode;

                        if (!isValidNeighbourNodeInOpenList)
                        {
                            openNodesList.Add(validNeighbourNode);
                        }
                    }
                }
            }
        }

    }

    /// <summary>
    /// 曼哈顿算法计算 距离
    /// </summary>
    private int GetDistance(Node NodeA, Node NodeB)
    {
        int dstX = Mathf.Abs(NodeA.gridPosition.x - NodeB.gridPosition.x);
        int dstY = Mathf.Abs(NodeA.gridPosition.y - NodeB.gridPosition.y);

        //对角线距离14 水平距离10
        if (dstX>dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }

        return 14 * dstX + 10 * (dstY - dstX);
    }

    /// <summary>
    /// 邻居节点的有效检测
    /// </summary>
    private Node GetValidNodeNeighbour(int neighboutNodeXPosition, int neighboutNodeYPosition)
    {
        //超出地图检测
        if (neighboutNodeXPosition>=gridWidth || neighboutNodeXPosition<0 || neighboutNodeYPosition >=gridHeight || neighboutNodeYPosition <0)
        {
            return null;
        }

        Node neighbourNode = gridNodes.GetGridNode(neighboutNodeXPosition, neighboutNodeYPosition);

        //障碍和重复节点 检测
        if (neighbourNode.isObstacle || closedNodeList.Contains(neighbourNode))
        {
            return null;
        }
        else
        {
            return neighbourNode;
        }
    }

    private bool populateGridNodesFromGridPropertiesDictionary(SceneName sceneName, Vector2Int startGridPosition, Vector2Int endGridPosition)
    {
        //获得场景的网格属性字典
        SceneSave sceneSave;

        if (GridPropertiesManager.Instance.GameObjectSave.sceneDate.TryGetValue(sceneName.ToString(),out sceneSave))
        {
            //获得网格属性细节
            if (sceneSave.gridPropertyDetailsDictionary !=null)
            {
                //获得网格高和宽
                if (GridPropertiesManager.Instance.GetGridDimensions(sceneName,out Vector2Int gridDimensions,out Vector2Int gridOrigin))
                {
                    //在网格属性字典上 创建节点网格
                    gridNodes = new GridNodes(gridDimensions.x, gridDimensions.y);
                    gridWidth = gridDimensions.x;
                    gridHeight = gridDimensions.y;
                    originX = gridOrigin.x;
                    originY = gridOrigin.y;

                    //初始化开放列表 和封闭列表
                    openNodesList = new List<Node>();
                    closedNodeList = new HashSet<Node>();
                }
                else
                {
                    return false;
                }

                //填充开始节点
                startNode = gridNodes.GetGridNode(startGridPosition.x - gridOrigin.x, startGridPosition.y - gridOrigin.y);

                //填充目标节点
                targetNode = gridNodes.GetGridNode(endGridPosition.x - gridOrigin.x, endGridPosition.y - gridOrigin.y);

                //填充障碍和路径到网格里
                for (int x = 0; x < gridDimensions.x; x++)
                {
                    for (int y = 0; y < gridDimensions.y; y++)
                    {
                        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(x + gridOrigin.x, y + gridOrigin.y);

                        if (gridPropertyDetails!=null)
                        {
                            //如果NPC被阻挡
                            if (gridPropertyDetails.isNPCObstacle==true)
                            {
                                Node node = gridNodes.GetGridNode(x, y);
                                node.isObstacle = true;
                            }
                            else if (gridPropertyDetails.isPath==true)
                            {
                                Node node = gridNodes.GetGridNode(x, y);
                                node.movementPenalty = pathMovementPenalty;
                            }
                            else
                            {
                                Node node = gridNodes.GetGridNode(x, y);
                                node.movementPenalty = defaultMovementPenalty;
                            }
                        }
                    }
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        return true;
    }
}
