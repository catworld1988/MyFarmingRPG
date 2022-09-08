using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [ItemCodeDescription] //自定义特性
    [SerializeField]
    private int _itemCode;

    private SpriteRenderer spriteRenderer;


    //ItemCode属性
    public int ItemCode
    {
        get { return _itemCode; }
        set { _itemCode = value; }
    }

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (ItemCode != 0)
        {
            Init(ItemCode);
        }
    }

    /// <summary>
    /// 给能收获的物件 添加碰撞晃动组件
    /// </summary>
    /// <param name="itemCodeParam"></param>
    public void Init(int itemCodeParam)
    {
        if (itemCodeParam != 0)
        {
            ItemCode = itemCodeParam;

            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(ItemCode);

            //确认是否是当前物体的小技巧
            spriteRenderer.sprite = itemDetails.itemSprite;

            //If item type is reapable then add Nudgeable component 给能收获的物件 添加碰撞晃动组件
            if (itemDetails.itemType == ItemType.Reapable_scenary)
            {
                gameObject.AddComponent<ItemNudge>();
            }
        }
    }
}