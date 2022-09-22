using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuInventoryManagementSlot : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerEnterHandler,IPointerExitHandler
{
    public Image inventoryManagementSlotImage;
    public TextMeshProUGUI TextMeshProUGUI;
    public GameObject greyedOutImageGO;
    [SerializeField] private PauseMenuInventoryManagement inventoryManagement = null;
    [SerializeField] private GameObject inventoryTextBoxPrefab = null;

    [HideInInspector] public ItemDetails itemDetails;
    [HideInInspector] public int itemQuantity;
    //槽位数量
    [SerializeField] public int slotNumber =0;

    //private Vector3 startingPosition;
    public GameObject draggedItem;
    private Canvas partentCanvas;

    private void Awake()
    {
        partentCanvas = GetComponentInParent<Canvas>();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        //if (itemQuantity != null)
        if (itemQuantity > 0)
        {
            draggedItem = Instantiate(inventoryManagement.inventoryManagementDraggedItemPrefab, inventoryManagement.transform);

            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();

            draggedItemImage.sprite = inventoryManagementSlotImage.sprite;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItem!=null)
        {
            draggedItem.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //销毁拖拽的物体
        if (draggedItem!=null)
        {
            Destroy(draggedItem);

            //对象拖拽结束
            if (eventData.pointerCurrentRaycast.gameObject!=null&& eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlot>()!=null)
            {
                //拖拽结束获得槽位数量
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlot>().slotNumber;

                //交换 库存物品
                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player,slotNumber,toSlotNumber);

                //销毁文本框
                inventoryManagement.DestroyInventoryTextBoxGameobject();


            }
        }

    }

    /// <summary>
    /// 鼠标指针进入
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Populate text box with item details
        //用项目详细信息填充文本框
        if (itemQuantity!=0)
        {
            //实例化文本框
            inventoryManagement.inventoryTextBoxGameobject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            inventoryManagement.inventoryTextBoxGameobject.transform.SetParent(partentCanvas.transform, false);

            UIInventoryTextBox inventoryTextBox = inventoryManagement.inventoryTextBoxGameobject.GetComponent<UIInventoryTextBox>();

            //设置无品的描述
            string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

            //填充文本框
            inventoryTextBox.SetTextBoxText(itemDetails.itemDescription,itemTypeDescription,"",itemDetails.itemDescription,"","");

            //设置文本框 位置
            if (slotNumber>23)
            {
                inventoryManagement.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
                inventoryManagement.inventoryTextBoxGameobject.transform.position =
                    new Vector3(transform.position.x, transform.position.y + 50f, transform.position.z);
            }
            else
            {
                inventoryManagement.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                inventoryManagement.inventoryTextBoxGameobject.transform.position =
                    new Vector3(transform.position.x, transform.position.y - 50f, transform.position.z);
            }
        }
    }

    /// <summary>
    /// 鼠标指针移出
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        inventoryManagement.DestroyInventoryTextBoxGameobject();
    }
}
