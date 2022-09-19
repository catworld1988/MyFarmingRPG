using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CropDetails
{
    [ItemCodeDescription] public int seedItemCode;  //这个作物的种子编号
    public int[] growthDays; //每个生长阶段的用几天
    //public int totalGrowthDays;  //总生长天数
    public GameObject[] growthPrefab;  //每一阶段生长的预制体
    public Sprite[] GrowthSprites;  //每个阶段的生长精灵图
    public Season[] Seasons;  //播种生长季节
    public Sprite harvestedSprite;  //收获的精灵图
    [ItemCodeDescription] public int harvestedTransformItemCode;  //如果该项在收获时转换为另一项，则将填充该项代码
    public bool hideCropBeforeHarvestedAnimation;  //收获动画结束前不能切割
    public bool disableCropCollidersBeforeHarvestedAnimation;  //收获动画前 关闭碰撞

    public bool isHarvestedAniamtion;  //是否 在最后生长阶段播放过收获动画  为真
    public bool isHarvestActionEffect = false;   //是否 标记  determine确定 是否有收获特效
    public bool spawnCropProducedAtPlayerPosition;  //是否 在玩家位置产生栽培
    public HarvestActionEffect harvestActionEffect;     //切割收获特效

    [ItemCodeDescription] public int[] harvestToolItemCode;     //可以收获该作物的物品编号的工具数组  如果没有装备工具 数组为0
    public int[] requiredHarvestAction;     //收获工具项目代码阵列中相应工具所需的收获动作
    [ItemCodeDescription] public int[] cropProducedItemCode;    //为收获的作物的收获物果实的一系列项目代码
    public int[] cropProducedMinQuantity;   //收获的作物最小数量
    public int[] cropProducedMaxQuantity;   //收获的作物最大数量
    public int daysToRegrow;    //再生下一茬作物的天数或-1(如果是单一作物)




    /// <summary>
    /// 如果工具的物品编码可以收获切割，就返回 true
    /// </summary>
    /// <param name="toolItemCode"></param>
    /// <returns></returns>
    public bool CanUseToolToHarvestCrop(int toolItemCode)
    {
        if (RequiredHarvestActionsForTool(toolItemCode)== -1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    //重新装备 收获动作
    public int RequiredHarvestActionsForTool(int toolItemCode)
    {
        //遍历查找 收获工具
        for (int i = 0; i < harvestToolItemCode.Length; i++)
        {
            //找到了收获工具
            if (harvestToolItemCode[i]==toolItemCode)
            {
                //从收获的动作 数组 返回动作
                return requiredHarvestAction[i];
            }
        }

        return -1;
    }
}
