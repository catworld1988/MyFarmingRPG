using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : SingletonMonobehaviour<Player>
{
    //喷壶动画的变量
    private WaitForSeconds afterLiftToolAniamtionPause;

    //锄地的变量
    private WaitForSeconds afterUseToolAnimationPause;

    //动画覆盖的变量
    private AnimationOverrides animationOverrides;

    //光标的变量
    private GridCursor gridCursor;
    private Cursor cursor;

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

    //喷壶的变量
    private WaitForSeconds liftToolAnimationPause;

    private Camera mainCamera;
    private bool playerToolUseDisabled = false;

    private ToolEffect toolEffect = ToolEffect.none;

    private Rigidbody2D rigidBody2D;
    private WaitForSeconds UseToolAnimationPause;

#pragma warning disable 414
    private Direction playerDirection;
#pragma warning restore 414

    //角色动画属性 定制列表
    private List<CharacterAttribute> characterAttributesCustomisationList;

    //显示举过头顶的工具
    [Tooltip("Should be populated in the prefab with the equipped item sprite renderer")] [SerializeField]
    private SpriteRenderer equippedItemSpriteRenderer = null;

    // 玩家手臂属性可以被替换
    private CharacterAttribute armsCharacterAttribute;
    private CharacterAttribute toolCharacterAttribute;

    private float movementSpeed;

    private bool _playerInputIsDisabled = false;

    //设置属性器 get私有
    public bool PlayerInputIsDisabled
    { get => _playerInputIsDisabled;
      set => _playerInputIsDisabled = value; }

    protected override void Awake()
    {
        base.Awake();
        rigidBody2D = GetComponent<Rigidbody2D>();

        animationOverrides = GetComponentInChildren<AnimationOverrides>();
        //初始化 可以替换的角色属性
        armsCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.arms, PartVariantColour.none, PartVariantType.none);
        toolCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.tool, PartVariantColour.none, PartVariantType.hoe);

        characterAttributesCustomisationList = new List<CharacterAttribute>();

        //获取主相机的参数
        mainCamera = Camera.main;
    }

    private void Start()
    {
        gridCursor = FindObjectOfType<GridCursor>();

        UseToolAnimationPause = new WaitForSeconds(Settings.useToolAniamtionPause);
        liftToolAnimationPause = new WaitForSeconds(Settings.liftToolAniamtionPause);
        afterUseToolAnimationPause = new WaitForSeconds(Settings.afterUseToolAniamtionPause);
        afterLiftToolAniamtionPause = new WaitForSeconds(Settings.afterLiftToolAniamtionPause);
    }

    private void Update()
    {
        #region Player Input

        //如果玩家没有被禁止输入 防止拖动道具 角色移动
        if (!PlayerInputIsDisabled)
        {
            //重置动画触发器
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


    /// <summary>
    /// 控制玩家是否输入
    /// </summary>
    private void PlayerClickInput()
    {
        if (!playerToolUseDisabled)
        {
            if (Input.GetMouseButton(0))
            {
                if (gridCursor.CursorIsEnabled)
                {
                    //获取光标网格位置
                    Vector3Int cursorGridPosition = gridCursor.GetGridPositionForCursor();

                    //获取玩家网格位置
                    Vector3Int playerGridPosition = gridCursor.GetGridPositionForPlayer();

                    ProcessPlayerClickInput(cursorGridPosition, playerGridPosition);
                }
            }
        }
    }

    private void ProcessPlayerClickInput(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        ResetMovement(); //停止移动

        Vector3Int playerDirection = GetPlayerClickDirection(cursorGridPosition, playerGridPosition); //获取玩家方向

        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        ItemDetails itemDetails = InventoryManager.Instance.GetselectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails != null)
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


                case ItemType.Watering_tool:
                case ItemType.Hoeing_tool: //使用的物品的类型是挖地工具
                    ProcessPlayerClickInputTool(gridPropertyDetails, itemDetails, playerDirection);
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


    private Vector3Int GetPlayerClickDirection(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        if (cursorGridPosition.x > playerGridPosition.x)
        {
            Debug.Log("点了右边");

            return Vector3Int.right;
        }

        else if (cursorGridPosition.x < playerGridPosition.x)
        {
            Debug.Log("点了左边");
            return Vector3Int.left;
        }

        else if (cursorGridPosition.y > playerGridPosition.y)
        {
            Debug.Log("点了上边");
            return Vector3Int.up;
        }
        else
        {
            Debug.Log("点了下边");
            return Vector3Int.down;
        }
    }


    private void ProcessPlayerClickInputSeed(ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorIsEnabled)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }

    private void ProcessPlayerClickInputCommodity(ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorIsEnabled)
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

    private void ProcessPlayerClickInputTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        //切换工具类型
        switch (itemDetails.itemType)
        {
            case ItemType.Hoeing_tool:
                if (gridCursor.CursorPositionIsVaild)
                {
                    //在鼠标位置锄地
                    HoeGroundAtCursor(gridPropertyDetails, playerDirection);
                }

                break;

            case ItemType.Watering_tool:
                if (gridCursor.CursorPositionIsVaild)
                {
                    WaterGroundAtCursor(gridPropertyDetails, playerDirection);
                }

                break;

            default:
                break;
        }
    }



    private void HoeGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        StartCoroutine(HoeGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }


    private IEnumerator HoeGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        PlayerInputIsDisabled = true; //禁止玩家输入
        playerToolUseDisabled = true; //禁止玩家使用工具

        //设置锄地动画 覆盖动画状态
        toolCharacterAttribute.partVariantType = PartVariantType.hoe;
        characterAttributesCustomisationList.Clear();
        characterAttributesCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList);

        if (playerDirection == Vector3Int.right)
        {
            isUsingToolRight = true;
        }
        else if (playerDirection == Vector3Int.left)
        {
            isUsingToolLeft = true;
        }
        else if (playerDirection == Vector3Int.up)
        {
            isUsingToolUp = true;
        }
        else if (playerDirection == Vector3Int.down)
        {
            isUsingToolDown = true;
        }

        //在使用工具动画 暂停
        yield return UseToolAnimationPause;

        //开始地块计时
        if (gridPropertyDetails.daysSinceDug == -1)
        {
            gridPropertyDetails.daysSinceDug = 0; //未来功能
        }

        //设置网格属性 被挖了
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        //显示被挖的瓦片
        GridPropertiesManager.Instance.DisplayDugGround(gridPropertyDetails);

        //在动画后 暂停
        yield return afterUseToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }


    private void WaterGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        //触发 动画
        StartCoroutine(WaterGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator WaterGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        //设置动画
        toolCharacterAttribute.partVariantType = PartVariantType.wateringCan;  //不同部位的类型
        characterAttributesCustomisationList.Clear();
        characterAttributesCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList);

        //TODO 设置水壶显示水
        toolEffect = ToolEffect.watering;

        //根据玩家方向 设置工具方向
        if (playerDirection == Vector3Int.right)
        {
            isLiftingToolRight = true;
        }
        else if (playerDirection == Vector3Int.left)
        {
            isLiftingToolLeft = true;
        }
        else if (playerDirection == Vector3Int.up)
        {
            isLiftingToolUp = true;
        }
        else if (playerDirection == Vector3Int.down)
        {
            isLiftingToolDown = true;
        }

        //工具动画前摇
        yield return liftToolAnimationPause;

        if (gridPropertyDetails.daysSinceWatered==-1)
        {
            gridPropertyDetails.daysSinceWatered = 0;
        }

        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        //动画后摇
        yield return afterLiftToolAniamtionPause;

        playerToolUseDisabled = false;
        PlayerInputIsDisabled = false;


    }


    ///TODO 删除
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
        PlayerInputIsDisabled = true;
    }

    //允许玩家输入
    public void EnablePlayerInput()
    {
        PlayerInputIsDisabled = false;
    }

    public void ClearCarriedItem()
    {
        equippedItemSpriteRenderer.sprite = null;
        equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);

        //属性重置 手臂自定义属性 切换回常规动画
        armsCharacterAttribute.partVariantType = PartVariantType.none;

        characterAttributesCustomisationList.Clear();
        characterAttributesCustomisationList.Add(armsCharacterAttribute);
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
            armsCharacterAttribute.partVariantType = PartVariantType.carry;

            characterAttributesCustomisationList.Clear();
            characterAttributesCustomisationList.Add(armsCharacterAttribute);
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