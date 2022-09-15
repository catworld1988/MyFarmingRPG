using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    private Canvas canvas;
    private Camera mainCamera;
    [SerializeField] private Image cursorImage = null;
    [SerializeField] private RectTransform cursorRectTransform = null;
    [SerializeField] private Sprite greenCursorSprite = null;
    [SerializeField] private Sprite transpartCursorSprite = null;
    [SerializeField] private GridCursor gridCursor = null;

    private bool _cursorIsEnabled = false;
    public bool CursorIsEnabled { get => _cursorIsEnabled; set => _cursorIsEnabled=value; }

    private bool _cursorPositionIsValid = false;
    public bool CursorPositionIsValid { get => _cursorPositionIsValid;set =>_cursorPositionIsValid = value; }


    private ItemType _selectedItemType;
    public ItemType SelectedItemType { get => _selectedItemType; set => _selectedItemType = value; }

    private float _itemUseRadius=0f;
    public float ItemUseRadius { get => _itemUseRadius; set => _itemUseRadius = value; }

    private void Start()
    {
        mainCamera=Camera.main;
        canvas = GetComponent<Canvas>();

    }

    private void Update()
    {
        if (CursorIsEnabled)
        {
            DisplayCursor();
        }
    }

    private void DisplayCursor()
    {
        //获得鼠标的世界坐标
        Vector3 cursorWorldPosition = GetWorldPositionForCursor();
        //设置鼠标的精灵图
        SetCursorValidity(cursorWorldPosition,Player.Instance.GetPlayerCentrePosition());
        //获得举行变换的位置
        cursorRectTransform.position=GetRectTransformPositionForCursor(); //
    }



    private void SetCursorValidity(Vector3 cursorPosition, Vector3 playerPosition)
    {
        SetCursorToValid();

        //人物可以只用工具的范围方形 角标 对角使用区域判断     左上Corner Point (x-r/2,y-r/2)  左上对角面积 cursorPosition.x <(playerPosition.x-ItemUseRadius/2f) && cursorPosition.y >(playerPosition.y ItemUseRadius 2f)

        //Check use radius corners 第一次检查
        if (cursorPosition.x > (playerPosition.x + ItemUseRadius / 2f) && cursorPosition.y > (playerPosition.y + ItemUseRadius / 2f)
            ||
            cursorPosition.x < (playerPosition.x - ItemUseRadius / 2f) && cursorPosition.y > (playerPosition.y + ItemUseRadius / 2f)
            ||
            cursorPosition.x < (playerPosition.x - ItemUseRadius / 2f) && cursorPosition.y < (playerPosition.y - ItemUseRadius / 2f)
            ||
            cursorPosition.x > (playerPosition.x - ItemUseRadius / 2f) && cursorPosition.y < (playerPosition.y - ItemUseRadius / 2f)
           )
        {
            SetCursorToInValid();  //游标设置为无效
            return;
        }

        //检查 工具范围外区域（另一种算法） 第二次检查
        if (Mathf.Abs(cursorPosition.x-playerPosition.x) > ItemUseRadius
            ||
            Mathf.Abs(cursorPosition.y-playerPosition.y) > ItemUseRadius)
        {
            SetCursorToValid(); //游标设置为无效
            return;

        }

        //检查能不能 获取选择的物品详情
        ItemDetails itemDetails = InventoryManager.Instance.GetselectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails == null)
        {
            SetCursorToInValid();  //游标设置为无效
            return;
        }


        switch (itemDetails.itemType)
        {
            case ItemType.Watering_tool:
            case ItemType.Breaking_tool:
            case ItemType.Chopping_tool:
            case ItemType.Hoeing_tool:
            case ItemType.Reaping_tool:
            case ItemType.Collecting_tool:
                if (!SetCursorValidityTool(cursorPosition,playerPosition,itemDetails))
                {
                    SetCursorToInValid();  //游标设置为无效
                    return;

                }
                break;
            case ItemType.none:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 游标   有效 显示
    /// </summary>
    private void SetCursorToValid()
    {
        cursorImage.sprite = greenCursorSprite;
        CursorPositionIsValid = true;

        gridCursor.DisableCursor(); //关闭网格光标 防止同时出现
    }


    /// <summary>
    /// 游标无效
    /// </summary>
    private void SetCursorToInValid()
    {
        cursorImage.sprite = transpartCursorSprite;
        CursorPositionIsValid = false;

        gridCursor.EnableCursor();  //打开网格光标

    }

    /// <summary>
    /// Sets the cursor as either valid or invalid for the tool for the target.Returns true if valid or false if invalid
    /// 将光标设置为对目标的工具有效或无效。如果有效则返回TRUE，如果无效则返回FALSE
    /// </summary>
    private bool SetCursorValidityTool(Vector3 cursorPosition, Vector3 playerPosition, ItemDetails itemDetails)
    {
        //切换切割工具的游标有效性
        switch (itemDetails.itemType)
        {
            case ItemType.Reaping_tool:
                return SetCursorValidityReapingTool(cursorPosition, playerPosition, itemDetails);

            default:
                return false;

        }
    }

    /// <summary>
    /// 设置切割工具的游标有效性
    /// </summary>
    private bool SetCursorValidityReapingTool(Vector3 cursorPosition, Vector3 playerPosition, ItemDetails equippedItemDetails)
    {
        List<Item> itemList = new List<Item>();

        if (HelperMethods.GetComponentsAtCursorLocation<Item>(out itemList,cursorPosition))
        {
            if (itemList.Count!=0)
            {
                foreach (Item item in itemList)
                {
                    //检查是不是风景 草木
                    if (InventoryManager.Instance.GetItemDetails(item.ItemCode).itemType== ItemType.Reapable_scenary)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }


    public void DisableCursor()
    {
        cursorImage.color = new Color(1f, 1f, 1f, 0f);
        CursorIsEnabled = false;
    }

    public void EnableCursor()
    {
        cursorImage.color = new Color(1f, 1f, 1f, 1f);
        CursorIsEnabled = true;
    }

    public Vector3 GetWorldPositionForCursor()
    {
        //获取鼠标位置
        Vector3 screenPosition=new Vector3(Input.mousePosition.x,Input.mousePosition.y,0f);

        //鼠标位置转换成 主摄像机世界坐标
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);

        return worldPosition;
    }

    private Vector3 GetRectTransformPositionForCursor()
    {
        Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        //将屏幕空间中的给定点转换为UI画布像素位置。
        return RectTransformUtility.PixelAdjustPoint(screenPosition,cursorRectTransform,canvas);
    }
}


