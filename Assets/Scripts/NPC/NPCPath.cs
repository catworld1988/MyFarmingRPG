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

            //如果堆栈>1 ,更新时间,然后在开始位置pop出. update times and then pop off ist item which is the starting position
            if (npcMovementStepStack.Count > 1)
            {
                UpdateTimesOnPath();
                npcMovementStepStack.Pop(); //移除顶部对象 丢弃

                //在NPC移动中设置时间表事件细节
                npcMovment.SetScheduleEventDetails(npcScheduleEvent);
            }
        }
    }

    /// <summary>
    /// 使用预期的游戏时间更新路径移动步骤
    /// </summary>
    private void UpdateTimesOnPath()
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