using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///目录管理器 加载   SO_ItemList ScriptableObject所有物品细节的目录信息。
/// </summary>
public class InventoryManager : SingletonMonobehaviour<InventoryManager>
{
    private Dictionary<int, ItemDetails> itemDetailsDictionary;

    [SerializeField]
    //调用物品的数据表
    private SO_ItemList itemList = null;

    protected override void Awake()
    {
        //创建 物品字典
        base.Awake();
        CreateItemDetailsDictionary();
    }


    /// <summary>
    /// Populates the itemDetailsDictionary from the scriptable object items list
    /// 将itemList 里的物品信息 填充到物品字典中
    /// </summary>
    private void CreateItemDetailsDictionary()
    {
        itemDetailsDictionary = new Dictionary<int, ItemDetails>();

        foreach (ItemDetails itemDetails in itemList.itemDetails)
        {
            itemDetailsDictionary.Add(itemDetails.itemCode, itemDetails);
        }
    }

    /// <summary>
    /// 尝试在物品字典中 用物品ID检索出物品其他详细信息
    /// </summary>
    /// <param name="itemCode"></param>
    /// <returns></returns>
    public ItemDetails GetItemDetails(int itemCode)
    {
        ItemDetails itemDetails;
        if (itemDetailsDictionary.TryGetValue(itemCode, out itemDetails))
        {
            return itemDetails;
        }
        else
        {
            return null;
        }
    }
}