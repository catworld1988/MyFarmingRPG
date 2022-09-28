using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : SingletonMonobehaviour<Player>, ISaveable
{
    //测试对象池的变量
    //public GameObject canyonOakTreePrefab;

    //捡农作物的变量
    private WaitForSeconds afterPickAniamtionPause;

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

    //捡农作物的
    private WaitForSeconds pickAniamtionPause;

    private Camera mainCamera;
    private bool playerToolUseDisabled = false;

    private ToolEffect toolEffect = ToolEffect.none;

    private Rigidbody2D rigidBody2D;
    private WaitForSeconds useToolAnimationPause;

    //玩家方向
    private Direction playerDirection;


    //角色动画属性 定制列表
    private List<CharacterAttribute> characterAttributeCustomisationList;

    //显示举过头顶的工具
    [Tooltip("Should be populated in the prefab with the equipped item sprite renderer")] [SerializeField]
    private SpriteRenderer equippedItemSpriteRenderer = null;

    // 玩家手臂属性可以被替换
    private CharacterAttribute armsCharacterAttribute;
    private CharacterAttribute toolCharacterAttribute;

    private float movementSpeed;

    //设置属性器 get私有
    private bool _playerInputIsDisabled = false;

    public bool PlayerInputIsDisabled
    { get => _playerInputIsDisabled;
      set => _playerInputIsDisabled = value; }


    private string _iSaveableUniqueID;

    public string ISaveableUniqueID
    { get => _iSaveableUniqueID;
      set => _iSaveableUniqueID = value; }


    private GameObjectSave _gameObjectSave;

    public GameObjectSave GameObjectSave
    { get => _gameObjectSave;
      set => _gameObjectSave = value; }

    protected override void Awake()
    {
        base.Awake();
        rigidBody2D = GetComponent<Rigidbody2D>();

        animationOverrides = GetComponentInChildren<AnimationOverrides>();
        //初始化 可以替换的角色属性
        armsCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.arms, PartVariantColour.none, PartVariantType.none);
        //初始化 割草工具等
        toolCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.tool, PartVariantColour.none, PartVariantType.hoe);

        characterAttributeCustomisationList = new List<CharacterAttribute>();
        //获得唯一标识符 便于存储数据
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;

        GameObjectSave = new GameObjectSave();

        //获取主相机的参数
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        ISaveableRegister();

        //防止切换场景 任务继续乱走
        EventHandler.BeforeSceneUnloadFadeOutEvent += DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent += EnablePlayerInput;
    }

    private void OnDisable()
    {
        ISaveableDeregister();

        //防止切换场景 任务继续乱走
        EventHandler.BeforeSceneUnloadFadeOutEvent -= DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent -= EnablePlayerInput;
    }

    private void Start()
    {
        //找到一个叫xx的对象
        gridCursor = FindObjectOfType<GridCursor>();
        cursor = FindObjectOfType<Cursor>();

        //等待的初始化 为了减少重复创建的开销
        useToolAnimationPause = new WaitForSeconds(Settings.useToolAniamtionPause);
        liftToolAnimationPause = new WaitForSeconds(Settings.liftToolAniamtionPause);
        pickAniamtionPause = new WaitForSeconds(Settings.pickAniamtionPause);
        afterUseToolAnimationPause = new WaitForSeconds(Settings.afterUseToolAniamtionPause);
        afterLiftToolAniamtionPause = new WaitForSeconds(Settings.afterLiftToolAniamtionPause);
        afterPickAniamtionPause = new WaitForSeconds(Settings.afterPickAniamtionPause);
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
                if (gridCursor.CursorIsEnabled || cursor.CursorIsEnabled)
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
                        ProcessPlayerClickInputSeed(gridPropertyDetails, itemDetails);
                    }

                    break;
                case ItemType.Commodity:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputCommodity(itemDetails);
                    }

                    break;


                case ItemType.Watering_tool:
                case ItemType.Breaking_tool:
                case ItemType.Chopping_tool:
                case ItemType.Hoeing_tool: //使用的物品的类型是挖地工具
                case ItemType.Reaping_tool:
                case ItemType.Collecting_tool:
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
            //Debug.Log("点了右边");

            return Vector3Int.right;
        }

        else if (cursorGridPosition.x < playerGridPosition.x)
        {
            //Debug.Log("点了左边");
            return Vector3Int.left;
        }

        else if (cursorGridPosition.y > playerGridPosition.y)
        {
            //Debug.Log("点了上边");
            return Vector3Int.up;
        }
        else
        {
            //Debug.Log("点了下边");
            return Vector3Int.down;
        }
    }


    private void ProcessPlayerClickInputSeed(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        //检查物品可以丢下 鼠标位置 地面挖过了 没有被种过
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid && gridPropertyDetails.daysSinceDug > -1 && gridPropertyDetails.seedItemCode == -1)
        {
            PlantSeedAtCursor(gridPropertyDetails, itemDetails);
        }

        else if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }

    private void PlantSeedAtCursor(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        //检测网格管理器是否 包含农作物的数据 防止出错
        if (GridPropertiesManager.Instance.GetCropDetails(itemDetails.itemCode) != null)
        {
            //更新网格属性上的 种子
            gridPropertyDetails.seedItemCode = itemDetails.itemCode;
            gridPropertyDetails.growthDays = 0;

            //显示种植的作物
            GridPropertiesManager.Instance.DisplayPlantedCrop(gridPropertyDetails);

            //从库存中移除 选择的物体
            EventHandler.CallRemoveSelectedItemFromInventoryEvent();

            //当玩家碰到 播放碰撞音效
            AudioManager.Instance.PlaySound(SoundName.effectPlantingSound);
        }
    }

    private void ProcessPlayerClickInputCommodity(ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
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
                if (gridCursor.CursorPositionIsValid)
                {
                    //在鼠标位置锄地
                    HoeGroundAtCursor(gridPropertyDetails, playerDirection);
                }

                break;

            case ItemType.Watering_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    WaterGroundAtCursor(gridPropertyDetails, playerDirection);
                }

                break;

            case ItemType.Chopping_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    ChopInplayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }

                break;


            case ItemType.Collecting_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    CollectInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }

                break;

            case ItemType.Breaking_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    BreakInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }

                break;


            case ItemType.Reaping_tool:
                if (cursor.CursorPositionIsValid)
                {
                    playerDirection = GetPlayerDirection(cursor.GetWorldPositionForCursor(), GetPlayerCentrePosition());
                    ReapInPlayerDirectionAtCursor(itemDetails, playerDirection);
                }

                break;

            default:
                break;
        }
    }

    private void BreakInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        //敲击音效
        AudioManager.Instance.PlaySound(SoundName.effectPixkaxe);

        //触发动画
        StartCoroutine(BreakInplayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));
    }

    private IEnumerator BreakInplayerDirectionRoutine(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        //设置割草动画覆盖
        toolCharacterAttribute.partVariantType = PartVariantType.pickaxe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        //在玩家方向使用 工具
        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return useToolAnimationPause;

        yield return afterPickAniamtionPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void ChopInplayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        //砍树音效
        AudioManager.Instance.PlaySound(SoundName.effectAxe);

        //触发动画
        StartCoroutine(ChopInplayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));
    }

    private IEnumerator ChopInplayerDirectionRoutine(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        //设置割草动画覆盖
        toolCharacterAttribute.partVariantType = PartVariantType.axe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        //在玩家方向割草
        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return useToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void CollectInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        //收集音效
        AudioManager.Instance.PlaySound(SoundName.effectBasket);

        StartCoroutine(CollectInPlayerDirectionAtCursorRoutine(gridPropertyDetails, itemDetails, playerDirection));
    }

    private IEnumerator CollectInPlayerDirectionAtCursorRoutine(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemItemDetails,
        Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemItemDetails, gridPropertyDetails);


        yield return pickAniamtionPause;

        yield return afterPickAniamtionPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }


    private void ReapInPlayerDirectionAtCursor(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        StartCoroutine(ReapInPlayerDirectionAtCursorRoutine(itemDetails, playerDirection));
    }

    private IEnumerator ReapInPlayerDirectionAtCursorRoutine(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        //设置割草动画覆盖
        toolCharacterAttribute.partVariantType = PartVariantType.scythe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        //在玩家方向割草
        UseToolInPlayerDirection(itemDetails, playerDirection);

        yield return useToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void UseToolInPlayerDirection(ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        if (Input.GetMouseButton(0))
        {
            switch (equippedItemDetails.itemType)
            {
                case ItemType.Reaping_tool:
                    if (playerDirection == Vector3Int.right)
                    {
                        isSwingingToolRight = true;
                    }
                    else if (playerDirection == Vector3Int.left)
                    {
                        isSwingingToolLeft = true;
                    }
                    else if (playerDirection == Vector3Int.up)
                    {
                        isSwingingToolUp = true;
                    }
                    else if (playerDirection == Vector3Int.down)
                    {
                        isSwingingToolDown = true;
                    }

                    break;
            }

            //Define centre point of square which will be used for collision testing
            //定义将用于碰撞测试的正方形中心点
            Vector2 point = new Vector2(GetPlayerCentrePosition().x + (playerDirection.x * (equippedItemDetails.itemUseRadius / 2f)),
                GetPlayerCentrePosition().y +
                playerDirection.y * (equippedItemDetails.itemUseRadius / 2f));

            //Define size of the square which will be used for collision testing
            //定义将用于碰撞测试的正方形的大小
            Vector2 size = new Vector2(equippedItemDetails.itemUseRadius, equippedItemDetails.itemUseRadius);

            //Get Item components with 2D collider located in the square at the centre point defined (2d colliders tested limited to maxCollidersToTestPerReapSwing)
            //获得项目组件与2D对撞机位于广场上，在定义的中心点（2d对撞机测试仅限于最大碰撞器）
            Item[] itemArray = HelperMethods.GetcomponentstBoxPositionNonAlloc<Item>(Settings.maxCollidersToTestPerReapSwing, point, size, 0f);

            int reapableItemCount = 0;


            //Loop through all items retrieved
            //循环遍历检索到的所有项目
            for (int i = itemArray.Length - 1; i >= 0; i--)
            {
                if (itemArray[i] != null)
                {
                    //销毁被割的对象
                    if (InventoryManager.Instance.GetItemDetails(itemArray[i].ItemCode).itemType == ItemType.Reapable_scenary)
                    {
                        //效果位置
                        Vector3 effectPosition = new Vector3(itemArray[i].transform.position.x,
                            itemArray[i].transform.position.y + Settings.gridCellSize / 2f, itemArray[i].transform.position.z);

                        //广播 收获时候触发的粒子效果
                        EventHandler.CallHarvestActionEffectEvent(effectPosition, HarvestActionEffect.reaping);

                        //镰刀音效
                        AudioManager.Instance.PlaySound(SoundName.effectScythe);

                        Destroy(itemArray[i].gameObject);

                        reapableItemCount++;

                        if (reapableItemCount >= Settings.maxTargetComponentsToDestroyPerReapSwing)
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 获取玩家使用割草工具的方向
    /// </summary>
    private Vector3Int GetPlayerDirection(Vector3 cursorPosition, Vector3 playerPosition)
    {
        //检查是否在使用右边的工具
        if (
            cursorPosition.x > playerPosition.x
            &&
            cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)
            &&
            cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)
        )
        {
            return Vector3Int.right;
        }

        else if (cursorPosition.x < playerPosition.x
                 &&
                 cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)
                 &&
                 cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)
                )
        {
            return Vector3Int.left;
        }
        else if (cursorPosition.y > playerPosition.y)
        {
            return Vector3Int.up;
        }
        else
        {
            return Vector3Int.down;
        }
    }


    private void HoeGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        //挖地音效
        AudioManager.Instance.PlaySound(SoundName.effectHoe);

        //触发挖地动画
        StartCoroutine(HoeGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }


    private IEnumerator HoeGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        PlayerInputIsDisabled = true; //禁止玩家输入
        playerToolUseDisabled = true; //禁止玩家使用工具

        //设置锄地动画 覆盖动画状态
        toolCharacterAttribute.partVariantType = PartVariantType.hoe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

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
        yield return useToolAnimationPause;

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
        //浇水音效
        AudioManager.Instance.PlaySound(SoundName.effectWateringCan);

        //触发 动画
        StartCoroutine(WaterGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator WaterGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        //设置动画
        toolCharacterAttribute.partVariantType = PartVariantType.wateringCan; //不同部位的类型
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

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

        if (gridPropertyDetails.daysSinceWatered == -1)
        {
            gridPropertyDetails.daysSinceWatered = 0;
        }

        //网格属性 设置为浇过水
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        //显示水的瓦片
        GridPropertiesManager.Instance.DisplayWateredGround(gridPropertyDetails);

        //动画后摇
        yield return afterLiftToolAniamtionPause;

        playerToolUseDisabled = false;
        PlayerInputIsDisabled = false;
    }


    /// <summary>
    /// Method processes crop with equipped item in player direction
    /// 一种在玩家方向处理装备物品的裁剪方法
    /// </summary>
    private void ProcessCropWithEquippedItemInPlayerDirection(Vector3Int playerDirection, ItemDetails equippedItemItemDetails,
        GridPropertyDetails gridPropertyDetails)
    {
        switch (equippedItemItemDetails.itemType)
        {
            case ItemType.Breaking_tool:
            case ItemType.Chopping_tool:

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

                break;


            case ItemType.Collecting_tool:

                if (playerDirection == Vector3Int.right)
                {
                    isPickingRight = true;
                }
                else if (playerDirection == Vector3Int.left)
                {
                    isPickingLeft = true;
                }
                else if (playerDirection == Vector3Int.up)
                {
                    isPickingUp = true;
                }
                else if (playerDirection == Vector3Int.down)
                {
                    isPickingDown = true;
                }

                break;

            default:
                break;
        }

        //从网格位置获得 农作物对象
        Crop crop = GridPropertiesManager.Instance.GetCropObjectAtGridLocation(gridPropertyDetails);

        //为作物执行流程工具操作
        if (crop != null)
        {
            switch (equippedItemItemDetails.itemType)
            {
                case ItemType.Chopping_tool:
                case ItemType.Breaking_tool:
                    crop.ProcessToolAction(equippedItemItemDetails, isUsingToolRight, isUsingToolLeft, isUsingToolDown, isUsingToolUp);
                    break;

                case ItemType.Collecting_tool:
                    crop.ProcessToolAction(equippedItemItemDetails, isPickingRight, isPickingLeft, isPickingDown, isPickingUp);
                    break;
            }
        }
    }


    ///TODO 删除  测试脚本!!!!!
    /// <summary>
    /// Temp routine for test input
    /// 测试!!!!! 输入的临时程序
    /// </summary>
    private void PlayerTestInput()
    {
        //测试  触发进阶增加 时间
        if (Input.GetKey(KeyCode.T))
        {
            TimeManager.Instance.TestAdvanceGameMinute();
        }

        //测试  触发进阶增加 天数
        if (Input.GetKey(KeyCode.G))
        {
            TimeManager.Instance.TestAdvanceGameGameDay();
        }

        //测试  触发 场景转换
        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneControllerManager.Instance.FadeAndLoadScene(SceneName.Scene1_Farm.ToString(), transform.position);
        }

        //测试 对象池
        /*if (Input.GetMouseButtonDown(1))
        {
            GameObject tree = PoolManager.Instance.ReuseObject(canyonOakTreePrefab,
                mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z)),Quaternion.identity);

            if (!tree.activeSelf)
            {
                tree.SetActive(true);
            }
        }*/
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

        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(armsCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList); //覆盖对应的动画控制器

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

            characterAttributeCustomisationList.Clear();
            characterAttributeCustomisationList.Add(armsCharacterAttribute);
            animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList); //覆盖对应的动画控制器

            isCarrying = true;
        }
    }


    public Vector3 GetPlayerViewportPosition()
    {
        //Vector3 viewport position for player ((0,0)viewport bottom left,(1,1)viewport top right
        //Vector3播放器的视区位置((0，0)视区左下，(1，1)视区右上   世界坐标转换成视口坐标
        return mainCamera.WorldToViewportPoint(transform.position);
    }

    public Vector3 GetPlayerCentrePosition()
    {
        //3维坐标 玩家轴心在底部，需要在轴心+y偏移量 为了之后触发工具动画的正确在腰部
        return new Vector3(transform.position.x, transform.position.y + Settings.playerCentreYOffset, transform.position.z);
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public GameObjectSave ISaveableSave()
    {
        //如果存在常驻场景 就移除它
        GameObjectSave.sceneDate.Remove(Settings.PersistentScene);

        //初始化变量
        SceneSave sceneSave = new SceneSave();
        sceneSave.vector3Dictionary = new Dictionary<string, Vector3Serializable>();
        sceneSave.stringDictionary = new Dictionary<string, string>();
        //存位置
        Vector3Serializable vector3Serializable = new Vector3Serializable(transform.position.x, transform.position.y, transform.position.z);
        sceneSave.vector3Dictionary.Add("playerPosition", vector3Serializable);
        //存场景名
        sceneSave.stringDictionary.Add("currentScene", SceneManager.GetActiveScene().name);
        //TODO 玩家方向不对
        //存玩家方向
        sceneSave.stringDictionary.Add("playerDirection", playerDirection.ToString());
        //打包成 上级游戏数据存
        GameObjectSave.sceneDate.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            if (gameObjectSave.sceneDate.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                if (sceneSave.vector3Dictionary != null && sceneSave.vector3Dictionary.TryGetValue("playerPosition", out Vector3Serializable playerPosition))
                {
                    transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
                }

                if (sceneSave.stringDictionary != null)
                {
                    if (sceneSave.stringDictionary.TryGetValue("currentScene", out string currentScene))
                    {
                        SceneControllerManager.Instance.FadeAndLoadScene(currentScene, transform.position);
                    }

                    if (sceneSave.stringDictionary.TryGetValue("playerDirection", out string playerDir))
                    {
                        bool playerDirFound = Enum.TryParse<Direction>(playerDir, true, out Direction direction);

                        if (playerDirFound)
                        {
                            playerDirection = direction;
                            SetPlayerDirection(playerDirection);
                        }
                    }
                }
            }
        }
    }

    public void ISaveableStoreScene(string sceneName)
    {
        //Nothing required here since the player is on a persistent scene;
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        //Nothing required here since the player is on a persistent scene;
    }


    private void SetPlayerDirection(Direction direction)
    {
        switch (playerDirection)
        {
            case Direction.up:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none, false, false, false, false,
                    false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false);
                break;

            case Direction.down:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none, false, false, false, false,
                    false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false);
                break;
            case Direction.left:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none, false, false, false, false,
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false);
                break;
            case Direction.right:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none, false, false, false, false,
                    false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true);

                break;

            default:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none, false, false, false, false,
                    false, false, false, false, false, false, false, false, false, false, false, false, false,true, false, false);

                break;
        }
    }
}