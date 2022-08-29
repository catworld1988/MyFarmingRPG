using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    /// <summary>
    /// 通过碰撞Trigger 物品获取物品 的信息
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item item = collision.GetComponent<Item>();

        if (item != null)
        {
            //碰撞体获取的物品ID  获得 物品信息
            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(item.ItemCode);

            //打印物品描述信息 到控制台
            Debug.Log(itemDetails.itemDescription);
        }
    }
}