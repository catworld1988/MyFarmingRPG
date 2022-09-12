using System.Collections.Generic;
using UnityEngine;

public class Player : SingletonMonobehaviour<Player>
{
    private AnimationOverrides animationOverrides;
    private GridCursor gridCursor;

    //Movement Parameters
    private float xInput;
    private float yInput;
    private bool isCarrying = false;
    private bool isIdle;
    private bool isLiftingToolDown;
    private bool isLiftingToolLeft;
    private bool isLiftingToolRight;
    private bool isLiftingToolUp;
    private bool isRunning;
    private bool isUsingToolDown;
    private bool isUsingToolLeft;
    private bool isUsingToolRight;
    private bool isUsingToolUp;
    private bool isSwingingToolDown;
    private bool isSwingingToolLeft;
    private bool isSwingingToolRight;
    private bool isSwingingToolUp;
    private bool isWalking;
    private bool isPickingUp;
    private bool isPickingDown;
    private bool isPickingLeft;
    private bool isPickingRight;

    private Camera mainCamera;

    private ToolEffect toolEffect = ToolEffect.none;

    private Rigidbody2D rigidBody2D;
#pragma warning disable 414
    private Direction playerDirection;
#pragma warning restore 414

    //角色动画属性 定制列表
    private List<CharacterAttribute> characterAttributesCustomisationList;

    //显示举过头顶的工具
    [Tooltip("Should be populated in the prefab with the equipped item sprite renderer")] [SerializeField]
    private SpriteRenderer equippedItemSpriteRenderer = null;

    // 玩家手臂属性可以被替换
    private CharacterAttribute armsCharacterAttributeAttribute;
    private CharacterAttribute toolCharacterAttributeAttribute;

    private float movementSpeed;

    private bool _playerInputIsDisable = false;

    //设置属性器 get私有
    public bool PlayerInputIsDisable
    { get => _playerInputIsDisable;
      set => _playerInputIsDisable = value; }

    protected override void Awake()
    {
        base.Awake();
        rigidBody2D = GetComponent<Rigidbody2D>();

        animationOverrides = GetComponentInChildren<AnimationOverrides>();
        //初始化 可以替换的角色属性
        armsCharacterAttributeAttribute = new CharacterAttribute(CharacterPartAnimator.arms, PartVariantColour.none, PartVariantType.none);

        characterAttributesCustomisationList = new List<CharacterAttribute>();

        //获取主相机的参数
        mainCamera = Camera.main;
    }

    private void Start()
    {
        gridCursor = FindObjectOfType<GridCursor>();
    }

    private void Update()
    {
        #region Player Input

        //如果玩家没有被禁止输入 防止拖动道具 角色移动
        if (!PlayerInputIsDisable)
        {
            ResetAnimationTriggers();

            PlayerMovementInput();

            PlayerWalkInput();

            PlayerClickInput();

            PlayerTestInput();



            //Send event to any listeners for player movement input  发送主角移动状态给EventHandle广播站  关闭了上下左右Idle
            EventHandler.CallMovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isCarrying, toolEffect,
                isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
                isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
                isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
                isSwingingToolRight, isSwingingToolLeft, isSwingingToolUp, isSwingingToolDown,
                false, false, false, false);
        }

        #endregion Player Input
    }


    private void FixedUpdate()
    {
        //实际的Player刚体运动 物理
        PlayerMovement();
    }


    /// <summary>
    /// 重置动画触发器
    /// </summary>
    private void ResetAnimationTriggers()
    {
        isPickingRight = false;
        isPickingLeft = false;
        isPickingUp = false;
        isPickingDown = false;
        isUsingToolRight = false;
        isUsingToolLeft = false;
        isUsingToolUp = false;
        isUsingToolDown = false;
        isLiftingToolRight = false;
        isLiftingToolLeft = false;
        isLiftingToolUp = false;
        isLiftingToolDown = false;
        isSwingingToolRight = false;
        isSwingingToolLeft = false;
        isSwingingToolUp = false;
        isSwingingToolDown = false;
        toolEffect = ToolEffect.none;
    }

    /// <summary>
    /// 移动输入
    /// </summary>
    private void PlayerMovementInput()
    {
        xInput = Input.GetAxis("Horizontal");
        yInput = Input.GetAxis("Vertical");

        //对角线运动 Pythagoras理论参数（勾股定理？）
        if (yInput != 0 && xInput != 0)
        {
            xInput = xInput * 0.71f;
            yInput = yInput * 0.71f;
        }

        if (xInput != 0 || yInput != 0)
        {
            isRunning = true;
            isWalking = false;
            isIdle = false;
            movementSpeed = Settings.runningSpeed;

            //保存玩家方向快照
            if (xInput < 0)
            {
                playerDirection = Direction.left;
            }
            else if (xInput > 0)
            {
                playerDirection = Direction.right;
            }
            else if (yInput < 0)
            {
                playerDirection = Direction.down;
            }
            else if (yInput > 0)
            {
                playerDirection = Direction.up;
            }
        }
        else if (xInput == 0 && yInput == 0)
        {
            isRunning = false;
            isWalking = false;
            isIdle = true;
        }
    }

    /// <summary>
    /// shift 步行
    /// </summary>
    private void PlayerWalkInput()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            isRunning = false;
            isWalking = true;
            isIdle = false;
            movementSpeed = Settings.walkingSpeed;
        }
        else
        {
            isRunning = true;
            isWalking = false;
            isIdle = false;
            movementSpeed = Settings.runningSpeed;
        }
    }

    private void PlayerClickInput()
    {
        if (Input.GetMouseButton(0))
        {
            if (gridCursor.CursorIsEnabled)
            {
                ProcessPlayerClickInput();
            }
        }
    }

    private void ProcessPlayerClickInput()
    {
        ResetMovement(); //停止移动

        ItemDetails itemDetails = InventoryManager.Instance.GetselectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails !=null)
        {
            switch (itemDetails.itemType)
            {
                case ItemType.Seed:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputSeed(itemDetails);
                    }
                    break;
                case ItemType.Commodity:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputCommodity(itemDetails);
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
    }



    private void ProcessPlayerClickInputSeed(ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped&& gridCursor.CursorIsEnabled)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }

    private void ProcessPlayerClickInputCommodity(ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped&& gridCursor.CursorIsEnabled)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }

    /// <summary>
    /// Player 的物理刚体移动
    /// </summary>
    private void PlayerMovement()
    {
        Vector2 move = new Vector2(xInput * movementSpeed * Time.deltaTime, yInput * movementSpeed * Time.deltaTime);
        rigidBody2D.MovePosition(rigidBody2D.position + move);
    }

    public void DisablePlayerInputAndResetMovement()
    {
        DisablePlayerInput();
        ResetMovement();

        //Send event to any listeners for player movement input  发送主角移动状态给EventHandle广播站  关闭了上下左右Idle
        EventHandler.CallMovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isCarrying, toolEffect,
            isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
            isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
            isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
            isSwingingToolRight, isSwingingToolLeft, isSwingingToolUp, isSwingingToolDown,
            false, false, false, false);
    }

    //TODO 删除
    /// <summary>
    /// Temp routine for test input
    /// 测试!!!!! 输入的临时程序
    /// </summary>
    private void PlayerTestInput()
    {
        //触发进阶增加 时间
        if (Input.GetKey(KeyCode.T))
        {
            TimeManager.Instance.TestAdvanceGameMinute();
        }

        //触发进阶增加 天数
        if (Input.GetKeyDown(KeyCode.G))
        {
            TimeManager.Instance.TestAdvanceGameGameDay();
        }

        //触发 场景转换
        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneControllerManager.Instance.FadeAndLoadScene(SceneName.Scene1_Farm.ToString(), transform.position);
        }
    }


    private void ResetMovement()
    {
        //Reset Movement 重置移动
        xInput = 0f;
        yInput = 0f;
        isRunning = false;
        isWalking = false;
        isIdle = true;
    }

    //禁止玩家输入
    public void DisablePlayerInput()
    {
        PlayerInputIsDisable = true;
    }

    //允许玩家输入
    public void EnablePlayerInput()
    {
        PlayerInputIsDisable = false;
    }

    public void ClearCarriedItem()
    {
        equippedItemSpriteRenderer.sprite = null;
        equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);

        //属性重置 手臂自定义属性 切换回常规动画
        armsCharacterAttributeAttribute.partVariantType = PartVariantType.none;

        characterAttributesCustomisationList.Clear();
        characterAttributesCustomisationList.Add(armsCharacterAttributeAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList); //覆盖对应的动画控制器

        isCarrying = false;
    }

    public void ShowCarriedItem(int itemCode)
    {
        //获取物品参数
        ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);
        if (itemDetails != null)
        {
            //设置显示图片
            equippedItemSpriteRenderer.sprite = itemDetails.itemSprite;
            equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);

            //属性变更 carry  手臂自定义属性
            armsCharacterAttributeAttribute.partVariantType = PartVariantType.carry;

            characterAttributesCustomisationList.Clear();
            characterAttributesCustomisationList.Add(armsCharacterAttributeAttribute);
            animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList); //覆盖对应的动画控制器

            isCarrying = true;
        }
    }


    public Vector3 GetPlayerViewportPosition()
    {
        //Vector3 viewport position for player ((0,0)viewport bottom left,(1,1)viewport top right
        //Vector3播放器的视区位置((0，0)视区左下，(1，1)视区右上   世界坐标转换成视口坐标
        return mainCamera.WorldToViewportPoint(transform.position);
    }
}