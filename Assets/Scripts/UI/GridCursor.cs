using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridCursor : MonoBehaviour
{
    private Canvas canvas;
    private Grid grid;
    private Camera mainCamera;

    [SerializeField] private Image cursorImage = null;
    [SerializeField] private RectTransform cursorRectTransform = null;
    [SerializeField] private Sprite greenCursorSprite = null;
    [SerializeField] private Sprite redCursorSprite = null;
    [SerializeField] private SO_CropDetailsList so_CropDetailsList = null;

    private bool _cursorPosition = false;

    public bool CursorPositionIsValid
    { get => _cursorPosition;
      set => _cursorPosition = value; }

    private int _itemUseGridRadius = 0;

    public int ItemUseGridRadius
    { get => _itemUseGridRadius;
      set => _itemUseGridRadius = value; }

    private ItemType _selectedItemType;

    public ItemType SelectedItemType
    { get => _selectedItemType;
      set => _selectedItemType = value; }

    private bool _cursorIsEnabled = false;

    public bool CursorIsEnabled
    { get => _cursorIsEnabled;
      set => _cursorIsEnabled = value; }


    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (CursorIsEnabled)
        {
            DisplayCursor();
        }
    }

    private Vector3Int DisplayCursor()
    {
        if (grid != null)
        {
            //根据鼠标获得格子的位置
            Vector3Int gridPosition = GetGridPositionForCursor();

            //根据玩家获得格子的位置
            Vector3Int playerGridPosition = GetGridPositionForPlayer();

            //光标的有效性
            SetCursorValidity(gridPosition, playerGridPosition);

            //基于网格位置转换成像素
            cursorRectTransform.position = GetRectTransformPositionForCursor(gridPosition);

            return gridPosition;
        }
        else
        {
            return Vector3Int.zero;
        }
    }

    void SceneLoaded()
    {
        grid = GameObject.FindObjectOfType<Grid>();
    }

    private void SetCursorValidity(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        SetCursorToValid();

        if (Mathf.Abs(cursorGridPosition.x - playerGridPosition.x) > ItemUseGridRadius ||
            Mathf.Abs(cursorGridPosition.y - playerGridPosition.y) > ItemUseGridRadius)
        {
            SetCursorToInValid();
            return;
        }

        ItemDetails itemDetails = InventoryManager.Instance.GetselectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails == null)
        {
            SetCursorToInValid();
            return;
        }

        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        if (gridPropertyDetails != null)
        {
            //Determinecursor validity based on inventory item selected andgrid property details
            //确定基于所选库存项目和网格属性详细信息的检查器有效性
            switch (itemDetails.itemType)
            {
                case ItemType.Seed:
                    if (!IsCursorValidForSeed(gridPropertyDetails))
                    {
                        SetCursorToInValid();
                        return;
                    }

                    break;
                case ItemType.Commodity:
                    if (!IsCursorValidForCommodity(gridPropertyDetails))
                    {
                        SetCursorToInValid();
                        return;
                    }

                    break;

                case ItemType.Watering_tool:
                case ItemType.Breaking_tool:
                case ItemType.Chopping_tool:
                case ItemType.Hoeing_tool:
                case ItemType.Reaping_tool:
                case ItemType.Collecting_tool:
                    if (!IsCursorValidForTool(gridPropertyDetails, itemDetails))
                    {
                        SetCursorToInValid();
                        return;
                    }

                    break;

                case ItemType.none:
                    break;
                case ItemType.count:
                    break;
                default:
                    break;
            }
        }
        else
        {
            SetCursorToInValid();
            return;
        }
    }


    private void SetCursorToInValid()
    {
        cursorImage.sprite = redCursorSprite;
        CursorPositionIsValid = false;
    }

    /// <summary>
    /// 绿色的光标（可使用）
    /// </summary>
    private void SetCursorToValid()
    {
        cursorImage.sprite = greenCursorSprite;
        CursorPositionIsValid = true;
    }

    private bool IsCursorValidForCommodity(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem;
    }


    /// <summary>
    /// 是否在 网格上 播种
    /// </summary>
    private bool IsCursorValidForSeed(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem; // 根据网格属性返回bool
    }

    /// <summary>
    /// Sets thecursoras either valid or invalid for the tool for the target gridPropertyDetails.Returns true if valid or false if invalid
    /// 为目标 gridPropertyDetails 的工具设置有效或无效的游标。如果有效返回 true，如果无效返回 false
    /// </summary>
    /// <param name="gridPropertyDetails"></param>
    /// <param name="itemDetails"></param>
    /// <returns></returns>
    private bool IsCursorValidForTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        //切换工具
        switch (itemDetails.itemType)
        {
            case ItemType.Hoeing_tool:
                if (gridPropertyDetails.isDiggable == true && gridPropertyDetails.daysSinceDug == -1)
                {
                    #region Need to get any items at location so we can check if they are reapable 需要在现场获取任何物品，以便我们可以检查它们是否可获得

                    //获得光标的世界坐标
                    Vector3 cursorWorldPosition = new Vector3(GetWorldPositionForCursor().x + 0.5f, GetWorldPositionForCursor().y + 0.5f, 0f);

                    //获得光标位置的物品列表
                    List<Item> itemList = new List<Item>();

                    HelperMethods.GetComponentsAtBoxLocation<Item>(out itemList, cursorWorldPosition, Settings.cursorSize, 0f);

                    #endregion

                    bool foundReapable = false;

                    foreach (Item item in itemList)
                    {
                        if (InventoryManager.Instance.GetItemDetails(item.ItemCode).itemType == ItemType.Reapable_scenary)
                        {
                            foundReapable = true;
                            break;
                        }
                    }

                    if (foundReapable)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }

            case ItemType.Watering_tool:
                //已经被挖过了 并且没有浇过水
                if (gridPropertyDetails.daysSinceDug > -1 && gridPropertyDetails.daysSinceWatered == -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }


            case ItemType.Collecting_tool:
            case ItemType.Chopping_tool:
            case ItemType.Breaking_tool:

                //如果种子已经种下了
                if (gridPropertyDetails.seedItemCode != -1)
                {
                    CropDetails cropDetails = so_CropDetailsList.GetCropDetails(gridPropertyDetails.seedItemCode);

                    //检查到作物
                    if (cropDetails != null)
                    {   //作物成熟了
                        if (gridPropertyDetails.growthDays >= cropDetails.growthDays[cropDetails.growthDays.Length-1])
                        {
                            //检测是能收获的工具
                            if (cropDetails.CanUseToolToHarvestCrop(itemDetails.itemCode))
                            {
                                return true;
                            }
                            else
                                return false;
                        }
                        else
                            return false;
                    }
                }
                return false;


            default:
                return false;
        }
    }


    /// <summary>
    /// DisableCursor is called in the UllnventorySlot.ClearCursors（）method when an inventoryslot item is no longer selected
    /// 当库存槽项目不再被选中时，在 UllnventorySlot.ClearCursor ()方法中调用 DisableCursor
    /// </summary>
    public void DisableCursor()
    {
        cursorImage.color = Color.clear;
        CursorIsEnabled = false;
    }

    /// <summary>
    /// EnableCursor is called in the UllnventorySlot.SetSelectedltem（）method when an inventoryslot item is selected and itsitemUseGrid radius>0
    /// 当选择库存槽项目且其 sitemUseGrid 半径 > 0时，在 UllnventorySlot. SetSelectedltem ()方法中调用 EnableCursor
    /// </summary>
    public void EnableCursor()
    {
        cursorImage.color = new Color(1f, 1f, 1f, 1f);
        CursorIsEnabled = true;
    }

    /// <summary>
    /// 根据鼠标的位置获得单元格的位置
    /// </summary>
    /// <returns></returns>
    public Vector3Int GetGridPositionForCursor()
    {
        //获取鼠标的世界坐标位置
        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));

        //转换为单元格的世界位置
        return grid.WorldToCell(worldPosition);
    }

    public Vector3Int GetGridPositionForPlayer()
    {
        return grid.WorldToCell(Player.Instance.transform.position);
    }

    /// <summary>
    /// 网格的屏幕位置
    /// </summary>
    public Vector2 GetRectTransformPositionForCursor(Vector3Int gridPosition)
    {
        Vector3 gridWorldPosition = grid.CellToWorld(gridPosition);
        Vector2 gridScreenPosition = mainCamera.WorldToScreenPoint(gridWorldPosition);
        return RectTransformUtility.PixelAdjustPoint(gridScreenPosition, cursorRectTransform, canvas);
    }

    public Vector3 GetWorldPositionForCursor()
    {
        return grid.CellToWorld(GetGridPositionForCursor());
    }
}