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

            /*//打印物品描述信息 到控制台
            Debug.Log(itemDetails.itemDescription);*/

            //如果物品属性是可以拾取的
            if (itemDetails.canBePickedUp==true)
            {
                //呼叫库存管理 将物品添加到库存中（库存位置，物品，对象）
                InventoryManager.Instance.AddItem(InventoryLocation.player,item,collision.gameObject);

                //当玩家碰到 播放捡起来音效
                AudioManager.Instance.PlaySound(SoundName.effectPickupSound);
            }
        }
    }
}