using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler,
    IPointerClickHandler
{
    //物品栏的实例参数
    public Image inventorySlotHighlight;
    public Image inventorySlotImage;
    public TextMeshProUGUI textMeshProUGUI;

    [HideInInspector] public ItemDetails itemDetails;
    [HideInInspector] public int itemQuantity;

    //拖放物体的实例参数
    private Camera mainCamera;
    private Transform parentItem;
    private GameObject draggedItem;

    [SerializeField] private UIInventoryBar inventoryBar = null;

    [SerializeField] private GameObject itemPrefab = null;

    //交换物品实例参数
    [SerializeField] private int slotNumber = 0;

    //描述框体的实例参数
    private Canvas parentCanvas;
    [SerializeField] private GameObject inventoryTextBoxPrefab = null;

    //是否被选中
    [HideInInspector] public bool isSelected = false;


    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
    }

    /// <summary>
    /// 在鼠标当前位置 拖动物品（如果选择了）。呼叫 DropItem event.
    /// </summary>
    /// <param name="eventData"></param>
    private void DropSelectedItemAtMousePosition()
    {
        if (itemDetails != null && isSelected)
        {
            //鼠标的屏幕坐标转换成世界坐标
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                Input.mousePosition.y, -mainCamera.transform.position.z));
            //在当前坐标,方向不变，父级目录下 创建物体预制体
            GameObject itemGameObject = Instantiate(itemPrefab, worldPosition, quaternion.identity, parentItem);
            Item item = itemGameObject.GetComponent<Item>();
            item.ItemCode = itemDetails.itemCode;

            //从玩家库存物品栏删除
            InventoryManager.Instance.RemoveItem(InventoryLocation.player, item.ItemCode);

            //如果没有物品 清除物品的选择
            if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.player,item.ItemCode)==-1)
            {
                ClearSelectedItem();
            }
        }
    }

    /// <summary>
    /// 拖拽开始的方法
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemDetails != null)
        {
            //Disable Player Keyboard input 关闭玩家输入
            Player.Instance.DisablePlayerInputAndResetMovement();

            //Instantiate gameobject as dragged item 实例化 物品栏上的物品容器（为拖动的物体准备）
            draggedItem = Instantiate(inventoryBar.inventoryBarDraggedItem, inventoryBar.transform);

            //Get image for dragged item 获得拖拽物体的图像
            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = inventorySlotImage.sprite;

            //高亮显示选择
            SetSelectedItem();
        }
    }

    /// <summary>
    /// 从物品栏拖动中的方法
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            //将被跟踪的项目的位置设置成为鼠标位置
            draggedItem.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //如果结束时 拖拽的物体存在
        if (draggedItem != null)
        {
            Destroy(draggedItem);
        }

        //如果当前指针射线 检测到物体 And 检测到该物体上有UIInventorySlot组件 同时 当前物品槽上有其他物品
        if (eventData.pointerCurrentRaycast.gameObject != null &&
            eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>() != null)
        {
            //拖拽结束后 获得拖拽处落点物品 槽位号
            int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>().slotNumber;

            //交换物品
            InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber);

            //防止交换的时候不要出现框体的BUG
            DestroyInventoryTextBox();

            //取消高亮显示选择
            ClearSelectedItem();
        }
        else
        {
            if (itemDetails.canBeDropped)
            {
                //丢在鼠标的位置 需要在方法内转换成为世界坐标
                DropSelectedItemAtMousePosition();
            }
        }

        //开启玩家输入
        Player.Instance.EnablePlayerInput();
    }

    /// <summary>
    /// 指针进入时执行
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        //先检测 是否存在物品
        if (itemQuantity != 0)
        {
            //实例化物品描述框体 设置父级 保存在UIInventoryBar里面
            inventoryBar.inventoryTextBoxGameobject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            inventoryBar.inventoryTextBoxGameobject.transform.SetParent(parentCanvas.transform, false);

            //获取描述框体 的文字脚本组件
            UIInventoryTextBox inventoryTextBox = inventoryBar.inventoryTextBoxGameobject.GetComponent<UIInventoryTextBox>();

            //设置物品的类型描述
            string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

            //将 物品描述，物品的类型描述，物品详细描述 填充进文字框
            inventoryTextBox.SetTextBoxText(itemDetails.itemDescription, itemTypeDescription, "", itemDetails.itemLongDescription, "", "");

            //根据物品栏的位置 判断描述框体的位置
            if (inventoryBar.IsInventoryBarPositionBottom)
            {
                inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
                inventoryBar.inventoryTextBoxGameobject.transform.position =
                    new Vector3(transform.position.x, transform.position.y + 50f, transform.position.z);
            }
            else
            {
                inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                inventoryBar.inventoryTextBoxGameobject.transform.position =
                    new Vector3(transform.position.x, transform.position.y - 50f, transform.position.z);
            }
        }
    }

    /// <summary>
    /// 指针推出时执行
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyInventoryTextBox();
    }

    /// <summary>
    /// 销毁文字描述实例框体
    /// </summary>
    public void DestroyInventoryTextBox()
    {
        if (inventoryBar.inventoryTextBoxGameobject != null)
        {
            Destroy(inventoryBar.inventoryTextBoxGameobject);
        }
    }

    /// <summary>
    /// 鼠标指针点击执行
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        //检测到按键数据 检测是否是鼠标左键点击
        if (eventData.button==PointerEventData.InputButton.Left)
        {
            //槽位是否选中状态
            if (isSelected==true)
            {
                ClearSelectedItem();
            }
            else
            {
                if (itemQuantity>0)
                {
                    SetSelectedItem();
                }
            }
        }

    }

    private void SetSelectedItem()
    {
        inventoryBar.ClearHighlightOnInventorySlots();
        isSelected = true;
        inventoryBar.SetHighlightedInventorySlots();

        InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player,itemDetails.itemCode);
    }

    private void ClearSelectedItem()
    {
        //清除物品高亮显示
        inventoryBar.ClearHighlightOnInventorySlots();
        isSelected = false;
        //在库存中设置无物品选中
        InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.player);
    }
}