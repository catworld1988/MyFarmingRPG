using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//时间表
[RequireComponent(typeof(NPCPath))]
public class NPCSchedule : MonoBehaviour
{
    [SerializeField] private SO_NPCScheduleEventList so_NpcScheduleEventList = null;
    private SortedSet<NPCScheduleEvent> npcScheduleEventSet;
    private NPCPath npcPath;

    private void Awake()
    {
        //读取NPC事件表事件列表
        //将NPC计划事件列表加载到已排序的集合中
        npcScheduleEventSet = new SortedSet<NPCScheduleEvent>(new NPCScheduleEventSort());

        foreach (NPCScheduleEvent npcScheduleEvent in so_NpcScheduleEventList.npcScheduleEventList)
        {
            //设置事件
            npcScheduleEventSet.Add(npcScheduleEvent);
        }

        //获取NPC路径组件
        npcPath = GetComponent<NPCPath>();
    }

    private void OnEnable()
    {
        EventHandler.AdvanceGameMinuteEvent += GameTimeSystem_AdvanceMinute;
    }

    private void OnDisable()
    {
        EventHandler.AdvanceGameMinuteEvent -= GameTimeSystem_AdvanceMinute;
    }

    private void GameTimeSystem_AdvanceMinute(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute, int gameSecond)
    {
        int time = (gameHour * 100) + gameMinute;

        //尝试获取匹配的时间表

        NPCScheduleEvent matchingNPCScheduleEvent = null;

        foreach (NPCScheduleEvent npcScheduleEvent in npcScheduleEventSet)
        {
            //时间匹配检查
            if (npcScheduleEvent.Time == time)
            {
                //检测参数是否匹配
                if (npcScheduleEvent.day !=0 && npcScheduleEvent.day != gameDay)
                    continue;

                if (npcScheduleEvent.season !=Season.none && npcScheduleEvent.season != gameSeason)
                    continue;

                if (npcScheduleEvent.weather != Weather.none && npcScheduleEvent.weather != GameManager.Instance.currentWeather)
                    continue;

                //时间表匹配
                matchingNPCScheduleEvent = npcScheduleEvent;
                break;
            }
            else if (npcScheduleEvent.Time > time)
            {
                break;
            }
        }

        //现在测试匹配的时间表 并执行
        if (matchingNPCScheduleEvent !=null)
        {
            //为匹配的时间表建立路径
            npcPath.BuildPath(matchingNPCScheduleEvent);
        }
    }
}
