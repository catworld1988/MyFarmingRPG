using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InventoryLocation //库存位置
{
    player,
    chest, //箱子
    count //统计数量
}
public enum ToolEffect
{
    none,
    watering
}

public enum Direction
{
    up,
    down,
    left,
    right,
    none
}

public enum ItemType
{
    Seed,
    Commodity,
    Watering_tool,
    Hoeing_tool,
    Chopping_tool,
    Breaking_tool,
    Reaping_tool,
    Collecting_tool,
    Reapable_scenary,
    Furniture,
    none,
    count
}