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

    public List<InventoryItem>[] inventoryLists;

    [HideInInspector] public int[] inventoryListCapacityInArray; //库存容量列表


    //调用物品的数据表
    [SerializeField] private SO_ItemList itemList = null;

    protected override void Awake()
    {
        base.Awake();

        //创建 库存列表
        CreateInventoryLists();


        //创建 物品字典
        CreateItemDetailsDictionary();
    }


    /// <summary>
    /// //创建 库存清单的方法
    /// </summary>
    private void CreateInventoryLists()
    {
        //库存数组的元素是库存位置的数量
        inventoryLists = new List<InventoryItem>[(int)InventoryLocation.count];

        //遍历数组，为每个元素创建一个库存列表  形成带有索引的库存列表
        for (int i = 0; i < (int)InventoryLocation.count; i++)
        {
            inventoryLists[i] = new List<InventoryItem>();
        }

        //initialise inventory list capacity array 初始化库存容量列表 指定每个库存列表可以容纳多少物品的整数
        inventoryListCapacityInArray = new int[(int)InventoryLocation.count];

        //initialise player inventory list capacity 初始化玩家库存容纳大小列表
        inventoryListCapacityInArray[(int)InventoryLocation.player] = Settings.playerInitialInventoryCapacity;
    }


    /// <summary>
    /// Populates the itemDetailsDictionary from the scriptable object items list
    /// 将itemList 里的物品信息 填充到物品字典中
    /// </summary>
    public void CreateItemDetailsDictionary()
    {
        itemDetailsDictionary = new Dictionary<int, ItemDetails>();

        foreach (ItemDetails itemDetails in itemList.itemDetails)
        {
            itemDetailsDictionary.Add(itemDetails.itemCode, itemDetails);
        }
    }

    /// <summary>
    /// Add an item to the inventory list for the inventoryLocation and then destroy the gameobject To Delete
    /// </summary>
    public void AddItem(InventoryLocation inventoryLocation, Item item, GameObject gameObjectToDelete)
    {
        AddItem(inventoryLocation, item);

        Destroy(gameObjectToDelete);
    }

    /// <summary>
    /// Add an item to the inventory list for the inventoryLocation 添加物品到库存列表
    /// </summary>
    public void AddItem(InventoryLocation inventoryLocation, Item item) //玩家库存0 箱子库存1 道具2
    {
        int itemCode = item.ItemCode;
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        //Check if inventory already contains the item 检查库存是否包换该物品
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            AddItemPosition(inventoryList, itemCode, itemPosition);
        }
        else
        {
            AddItemPosition(inventoryList, itemCode);
        }

        //Send event that inventory has been updated   呼叫事件管理中心  发布库存更新事件广播
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    /// <summary>
    /// 将包含itemCode 和数量 的物品结构体 添加到物品库存清单中末尾
    /// </summary>
    private void AddItemPosition(List<InventoryItem> inventoryList, int itemCode)
    {
        InventoryItem inventoryItem = new InventoryItem();

        inventoryItem.itemCode = itemCode;
        inventoryItem.itemQuantity = 1; //库存数量1
        inventoryList.Add(inventoryItem);

        //DebugPrintInventoryList(inventoryList);
    }

    /// <summary>
    /// 增加同一物品的库存
    /// </summary>
    private void AddItemPosition(List<InventoryItem> inventoryList, int itemCode, int Position)
    {
        //创建一个新物品；
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[Position].itemQuantity + 1; //当前清单索引位置 的物品库存数量+1
        inventoryItem.itemQuantity = quantity;
        inventoryItem.itemCode = itemCode;
        inventoryList[Position] = inventoryItem; //刷新下物品

        Debug.ClearDeveloperConsole();
        //DebugPrintInventoryList(inventoryList);
    }

    /// <summary>
    /// Find if an itemCode is already in the inventory.Returns the item position in the inventory list
    /// ,or -1 if the item is not in the inventory
    /// 用库存位置和物品itemCode在物品库存清单中查找该物品 找到返回物品清单列表索引
    /// </summary>
    /// <param name="inventoryLocation"></param>
    /// <param name="itemCode"></param>
    /// <returns></returns>
    private int FindItemInInventory(InventoryLocation inventoryLocation, int itemCode)
    {
        //创建库存清单 并分配库存清单位置 玩家库存0 箱子库存1 道具库存2
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        for (int i = 0; i < inventoryList.Count; i++)
        {
            if (inventoryList[i].itemCode == itemCode)
            {
                //找到了就返回物品清单列表索引
                return i;
            }
        }

        //没有找到返回-1
        return -1;
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

    /// <summary>
    /// 打印物品库存清单
    /// </summary>
    // private void DebugPrintInventoryList(List<InventoryItem> inventoryList)
    // {
    //     foreach (InventoryItem inventoryItem in inventoryList)
    //     {
    //         Debug.Log("Item Description:" +
    //                   InventoryManager.Instance.GetItemDetails(inventoryItem.itemCode).itemDescription +
    //                   "    Item Quantity:" + inventoryItem.itemQuantity);
    //     }
    //
    //     Debug.Log("***************************************************************************");
    // }
}