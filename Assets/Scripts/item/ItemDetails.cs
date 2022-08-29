using UnityEngine;


[System.Serializable]
//物品信息
public class ItemDetails
{
    public int itemCode;
    public ItemType itemType;
    public string itemDescription; //物品简单描述
    public Sprite itemSprite;
    public string itemLongDescription;
    public short itemUseGridRadius; //物品占用格子
    public float itemUseRadius;
    public bool isStartingItem;
    public bool canBePickedUp;
    public bool canBeDropped;
    public bool canBeEaten;
    public bool canBeCarried;
}
