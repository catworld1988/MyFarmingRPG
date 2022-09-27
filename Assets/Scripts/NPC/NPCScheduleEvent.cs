using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// NPC时间表事件
/// </summary>
[System.Serializable]
public class NPCScheduleEvent
{
    public int hour;
    public int minute;
    public int priority; //优先级
    public int day;
    public Weather weather;
    public Season season;
    public SceneName toSceneName;
    public GridCoordinate toGridCoordinate;
    public Direction npcFacingDirectionAtDestination = Direction.none;
    public AnimationClip animationAtDestination;

    public int Time
    {
        //转换成数字串
        get
        {
            return (hour * 100) + minute;
        }
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    public NPCScheduleEvent(int hour, int minute, int priority, int day, Weather weather, Season season, SceneName toSceneName, GridCoordinate toGridCoordinate,
        AnimationClip animationAtDestination)
    {
        this.hour = hour;
        this.minute = minute;
        this.priority = priority;
        this.day = day;
        this.weather = weather;
        this.season = season;
        this.toSceneName = toSceneName;
        this.toGridCoordinate = toGridCoordinate;
        this.animationAtDestination = animationAtDestination;
    }

    public NPCScheduleEvent()
    {

    }

    public override string ToString()
    {
        return $"Time:{Time}, Priority:{priority}, Day:{day} Weather:{weather}, Season{season}";
    }
}
