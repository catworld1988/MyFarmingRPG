using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInventoryBar : MonoBehaviour
{
    [SerializeField] private Sprite blank16x16sprite = null;
    [SerializeField] private UIInventorySlot[] inventorySlots = null;
    public GameObject inventoryBarDraggedItem;
    [HideInInspector] public GameObject inventoryTextBoxGameobject; //用来存储描述框体的实例

    private RectTransform rectTransform;

    private bool _isInventoryBarPositionBottom = true;

    public bool IsInventoryBarPositionBottom
    {
        get => _isInventoryBarPositionBottom;
        set => _isInventoryBarPositionBottom = value;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        EventHandler.InventoryUpdatedEvent += InventoryUpdated;
    }

    private void OnDisable()
    {
        EventHandler.InventoryUpdatedEvent -= InventoryUpdated;
    }

    private void Update()
    {
        //根据玩家位置切换 道具栏位置
        SwitchInventoryBarPosition();
    }

    /// <summary>
    /// 清除所有高光 在物品栏
    /// </summary>
    public void ClearHighlightOnInventorySlots()
    {
        if (inventorySlots.Length > 0)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                //关闭高光素材 设置bool值
                if (inventorySlots[i].isSelected)
                {
                    inventorySlots[i].isSelected = false;
                    inventorySlots[i].inventorySlotHighlight.color = new Color(0f, 0f, 0f,0f);
                    //更新物品栏为没有选中状态
                    InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.player);
                }
            }
        }
    }


    //库存更新
    private void InventoryUpdated(InventoryLocation inventoryLocation, List<InventoryItem> inventoryList)
    {
        if (inventoryLocation == InventoryLocation.player)
        {
            ClearInventorySlots();

            //检查是否有 可用的库存槽位 && 当前玩家获得道具数量>0
            if (inventorySlots.Length > 0 && inventoryList.Count > 0)
            {
                //loop through inventory slots and update with corresponding inventory list item
                //循环库存槽 和更新库存列表项
                for (int i = 0; i < inventorySlots.Length; i++)
                {
                    if (i < inventoryList.Count)
                    {
                        int itemCode = inventoryList[i].itemCode;

                        //ItemDetails itemDetails=InventoryManager.Instance.itemList.itemDetails.Find(x => x.itemCode == itemCode);
                        ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);

                        if (itemDetails != null)
                        {
                            //add image and details to inventory item slot
                            //在物品槽位 添加图片和描述细节
                            inventorySlots[i].InventorySlotImage.sprite = itemDetails.itemSprite;
                            inventorySlots[i].textMeshProUGUI.text = inventoryList[i].itemQuantity.ToString();
                            inventorySlots[i].itemDetails = itemDetails;
                            inventorySlots[i].itemQuantity = inventoryList[i].itemQuantity;

                            SetHighlightedInventorySlots(i);

                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }

    //清除 库存槽位 为初始状态
    private void ClearInventorySlots()
    {
        if (inventorySlots.Length > 0)
        {
            //循环重置槽位为空的槽位 图片重置为空白图
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                inventorySlots[i].InventorySlotImage.sprite = blank16x16sprite;
                inventorySlots[i].textMeshProUGUI.text = "";
                inventorySlots[i].itemDetails = null;
                inventorySlots[i].itemQuantity = 0;

                SetHighlightedInventorySlots(i);

            }
        }
    }


    /// <summary>
    /// Set the selected highlight if set on all inventory item positions
    /// 设置选中的高亮显示，如果设置在所有库存物品的位置
    /// </summary>
    public void SetHighlightedInventorySlots()
    {
        if (inventorySlots.Length>0)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                SetHighlightedInventorySlots(i);
            }
        }
    }

    /// <summary>
    /// Set the selected highlight if set on an inventory item for a given slot item position
    /// 设置选中的高亮显示，如果设置在一个给定的插槽项目位置的库存项目
    /// </summary>
    /// <param name="itemPosition"></param>
    public void SetHighlightedInventorySlots(int itemPosition)
    {
        if (inventorySlots.Length>0 && inventorySlots[itemPosition].itemDetails !=null)
        {
            if (inventorySlots[itemPosition].isSelected)
            {
                inventorySlots[itemPosition].inventorySlotHighlight.color = new Color(1f, 1f, 1f, 1f); //alpha可见 最好用canvas组件控制 优化更好

                //更新显示物品被选中
                InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player,inventorySlots[itemPosition].itemDetails.itemCode);
            }
        }
    }



    private void SwitchInventoryBarPosition()
    {
        Vector3 playerViewportPosition = Player.Instance.GetPlayerViewportPosition();

        if (playerViewportPosition.y > 0.3f && IsInventoryBarPositionBottom == false)
        {
            //Debug.Log("道具栏在下面");
            //transform.position=new Vector3(transform.position.x,7.5f,0f);
            //this was changed to control the rectTransform see below  变换位置和锚点 到屏幕下方 适配不用屏幕
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0.5f, 2.5f);

            IsInventoryBarPositionBottom = true;
        }
        else if (playerViewportPosition.y <= 0.3f && IsInventoryBarPositionBottom == true)
        {
            //Debug.Log("道具栏在上面");
            //transform.position new Vector3(transform.position.x,mainCamera.pixelHeight 120f,0f);
            //this was changed to control the rectTransform see below  变换位置和锚点 到屏幕上方 适配不用屏幕
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0f, -2.5f);

            IsInventoryBarPositionBottom = false;
        }
    }
}