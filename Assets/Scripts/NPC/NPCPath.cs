using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCMovement))]
public class NPCPath : MonoBehaviour
{
    public Stack<NPCMovementStep> npcMovementStepStack;

    private NPCMovement npcMovment;

    private void Awake()
    {
        npcMovment = GetComponent<NPCMovement>();
        npcMovementStepStack = new Stack<NPCMovementStep>();
    }

    public void ClearPath()
    {
        npcMovementStepStack.Clear();
    }

    public void BuildPath(NPCScheduleEvent npcScheduleEvent)
    {
        ClearPath();

        //如果时间表的场景和NPC当前场景一致
        if (npcScheduleEvent.toSceneName == npcMovment.npcCurrentScene)
        {
            Vector2Int npcCurrentGridPosition = (Vector2Int)npcMovment.npcCurrentGridPosition;

            Vector2Int npcTargetGridPosition = (Vector2Int)npcScheduleEvent.toGridCoordinate;

            //建立寻路路径 和添加 移动步骤 到堆栈
            NPCManager.Instance.BuildPath(npcScheduleEvent.toSceneName, npcCurrentGridPosition, npcTargetGridPosition, npcMovementStepStack);

        }
        ////如果时间表的场景和NPC当前场景 不一致
        else if(npcScheduleEvent.toSceneName!= npcMovment.npcCurrentScene)
        {
            SceneRoute sceneRoute;

            //获得匹配时间表的场景路线
            sceneRoute = NPCManager.Instance.GetSceneRoute(npcMovment.npcCurrentScene.ToString(), npcScheduleEvent.toSceneName.ToString());

            //找到路线?
            if (sceneRoute!=null)
            {
                //反向遍历路径
                for (int i = sceneRoute.scenePathList.Count-1; i >=0; i--)
                {
                    int toGridX, toGridY, fromGridX, fromGridY;

                    ScenePath scenePath = sceneRoute.scenePathList[i];

                    //检查是否最终目的地 目标场景的x,y设定为999999 用来判定是否到达,到达则改回目标地点的坐标
                    if (scenePath.toGridCell.x >= Settings.maxGridWidth || scenePath.toGridCell.y >= Settings.maxGridHeight)
                    {
                        toGridX = npcScheduleEvent.toGridCoordinate.x;
                        toGridY = npcScheduleEvent.toGridCoordinate.y;
                    }
                    else
                    {
                        //不是的话使用 前往切换场景坐标
                        toGridX = scenePath.toGridCell.x;
                        toGridY = scenePath.toGridCell.y;
                    }

                    //检查是否是起始场景
                    if (scenePath.fromGridCell.x >= Settings.maxGridWidth || scenePath.fromGridCell.y >= Settings.maxGridHeight)
                    {
                        //是的话 使用当前坐标
                        fromGridX = npcMovment.npcCurrentGridPosition.x;
                        fromGridY = npcMovment.npcCurrentGridPosition.y;
                    }
                    else
                    {
                        //不是的话使用 来自切换场景坐标
                        fromGridX = scenePath.fromGridCell.x;
                        fromGridY = scenePath.fromGridCell.y;
                    }

                    Vector2Int fromGridPosition = new Vector2Int(fromGridX, fromGridY);

                    Vector2Int toGridPosition = new Vector2Int(toGridX, toGridY);

                    //建立路径和添加移动步骤到移动堆栈
                    NPCManager.Instance.BuildPath(scenePath.sceneName, fromGridPosition, toGridPosition, npcMovementStepStack);
                }
            }
        }

        //如果堆栈>1 ,更新时间,然后在开始位置pop出. update times and then pop off ist item which is the starting position
        if (npcMovementStepStack.Count > 1)
        {
            UpdateTimesOnPath();
            npcMovementStepStack.Pop(); //移除顶部对象 丢弃

            //在NPC移动中设置时间表事件细节
            npcMovment.SetScheduleEventDetails(npcScheduleEvent);
        }
    }

    /// <summary>
    /// 使用预期的游戏时间更新路径移动步骤
    /// </summary>
    public void UpdateTimesOnPath()
    {
        //获得当前游戏时间 时间跨度
        TimeSpan currentGameTime = TimeManager.Instance.GetGameTime();

        //以前的NPC移动步骤
        NPCMovementStep previousNPCMovementStep = null;

        foreach (NPCMovementStep npcMovementStep in npcMovementStepStack)
        {
            if (previousNPCMovementStep == null)
            {
                previousNPCMovementStep = npcMovementStep;
            }

            npcMovementStep.hour = currentGameTime.Hours;
            npcMovementStep.minute = currentGameTime.Minutes;
            npcMovementStep.second = currentGameTime.Seconds;

            //运动每步用时间隔
            TimeSpan movementTimeStep;

            //如果移动对角
            if (MovementIsDiagonal(npcMovementStep, previousNPCMovementStep))
            {
                movementTimeStep = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / Settings.secondsPerGameSecond / npcMovment.npcNormalSpeed));
            }
            else
            {
                movementTimeStep = new TimeSpan(0, 0, (int)(Settings.gridCellSize / Settings.secondsPerGameSecond / npcMovment.npcNormalSpeed));
            }

            currentGameTime = currentGameTime.Add(movementTimeStep);

            previousNPCMovementStep = npcMovementStep;
        }
    }

    /// <summary>
    /// returns true if the previous movement step is diagonal to movement step,else returns false.
    /// 如果上一个移动步骤与当前移动步骤是对角线，则返回true，否则返回false.
    /// </summary>
    private bool MovementIsDiagonal(NPCMovementStep npcMovementStep, NPCMovementStep previousNpcMovementStep)
    {
        if ((npcMovementStep.gridCoordinate.x != previousNpcMovementStep.gridCoordinate.x) && (npcMovementStep.gridCoordinate.y != previousNpcMovementStep.gridCoordinate.y))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


}