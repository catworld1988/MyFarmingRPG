using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//自定义排序:npc时间表按照时间排序
public class NPCScheduleEventSort : IComparer<NPCScheduleEvent>
{
    public int Compare(NPCScheduleEvent npcScheduleEvent1, NPCScheduleEvent npcScheduleEvent2)
    {
        // 检查 ?是否为null
        //时间相等 按照优先级排序
        if (npcScheduleEvent1?.Time == npcScheduleEvent2?.Time)
        {
            if (npcScheduleEvent1?.priority < npcScheduleEvent2?.priority)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
        //时间不等 按照时间排序
        else if (npcScheduleEvent1?.Time > npcScheduleEvent2?.Time)
        {
            return 1;
        }else if (npcScheduleEvent1?.Time < npcScheduleEvent2?.Time)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}
