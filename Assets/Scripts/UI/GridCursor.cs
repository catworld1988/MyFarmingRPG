using System.Collections;
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

    private bool _cursorPosition = false;

    public bool CursorPositionIsVaild
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

        if (gridPropertyDetails !=null)
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
        CursorPositionIsVaild = false ;

    }
    /// <summary>
    /// 绿色的光标（可使用）
    /// </summary>
    private void SetCursorToValid()
    {
        cursorImage.sprite = greenCursorSprite;
        CursorPositionIsVaild = true;

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
        cursorImage.color = new Color(1f,1f,1f,1f);
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
            mainCamera.ScreenToWorldPoint(new UnityEngine.Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));

        //转换为单元格的世界位置
        return grid.WorldToCell(worldPosition);
    }

    private Vector3Int GetGridPositionForPlayer()
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

}