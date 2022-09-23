using UnityEngine;

public static class Settings
{
    //Scenes
    public const string PersistentScene = "PersistentScene";

    //Obscuring Item Fading - ObscuringItemFader 遮挡物体的淡入淡出
    public const float fadeInSeconds = 0.25f;
    public const float fadeOutSeconds = 0.35f;
    public const float targetAlpha = 0.45f;

    //瓦片地图            单元格为 1个单位
    public const float gridCellSize = 1f;
    public static Vector2 cursorSize = Vector2.one;

    //
    public static float playerCentreYOffset =0.875f; //大概11个像素 一个单位高16个像素

    //Player Movement
    //public const float runningSpeed = 5.333f;
    //TODO 测试使用 需要改回常量
    public const float runningSpeed = 13f;
    public const float walkingSpeed = 2.666f;
    public static float useToolAniamtionPause = 0.25f;
    public static float liftToolAniamtionPause= 0.4f;
    public static float pickAniamtionPause= 1f;
    public static float afterUseToolAniamtionPause = 0.2f;
    public static float afterLiftToolAniamtionPause = 0.4f;
    public static float afterPickAniamtionPause= 0.2f;


    //Inventory
    public static int playerInitialInventoryCapacity = 24; //初始库存容量
    public static int playerMaximumInventoryCapacity = 48; //最大库存容量


    // Player Animation Parameters

    #region Player Animation Parameters

    public static int xInput;
    public static int yInput;
    public static int isWalking;
    public static int isRunning;
    public static int toolEffect;
    public static int isUsingToolRight;
    public static int isUsingToolLeft;
    public static int isUsingToolUp;
    public static int isUsingToolDown;
    public static int isLiftingToolRight;
    public static int isLiftingToolLeft;
    public static int isLiftingToolUp;
    public static int isLiftingToolDown;
    public static int isSwingingToolRight;
    public static int isSwingingToolLeft;
    public static int isSwingingToolUp;
    public static int isSwingingToolDown;
    public static int isPickingRight;
    public static int isPickingLeft;
    public static int isPickingUp;
    public static int isPickingDown;

    #endregion


    //Shared Animation Parameters
    public static int idleUp;
    public static int idleDown;
    public static int idleLeft;
    public static int idleRight;

    //Tools
    public const string HoeingTool = "Hoe";
    public const string ChoppingTool = "Axe";
    public const string BreakingTool = "Pickaxe";
    public const string ReapingTool = "Scythe";
    public const string WateringTool = "Watering Can";
    public const string CollectingTool = "Basket";

    //Reaping 割草时候
    public const int maxCollidersToTestPerReapSwing = 15;  //割草范围
    public const int maxTargetComponentsToDestroyPerReapSwing = 2;  //割草数量



    //Time System 时间系统
    public const float secondsPerGameSecond = 0.012f;   //0.7秒=1分钟 因为实际游戏中时间比现实中更快 效率更高

    //静态构造函数
    static Settings()
    {
        //player Animation Parameters
        xInput = Animator.StringToHash("xInput");
        yInput = Animator.StringToHash("yInput");
        isWalking = Animator.StringToHash("isWalking");
        isRunning = Animator.StringToHash("isRunning");
        toolEffect = Animator.StringToHash("toolEffect");
        isUsingToolRight = Animator.StringToHash("isUsingToolRight");
        isUsingToolLeft = Animator.StringToHash("isUsingToolLeft");
        isUsingToolUp = Animator.StringToHash("isUsingToolUp");
        isUsingToolDown = Animator.StringToHash("isUsingToolDown");
        isLiftingToolRight = Animator.StringToHash("isLiftingToolRight");
        isLiftingToolLeft = Animator.StringToHash("isLiftingToolLeft");
        isLiftingToolUp = Animator.StringToHash("isLiftingToolUp");
        isLiftingToolDown = Animator.StringToHash("isLiftingToolDown");
        isSwingingToolRight = Animator.StringToHash("isSwingingToolRight");
        isSwingingToolLeft = Animator.StringToHash("isSwingingToolLeft");
        isSwingingToolUp = Animator.StringToHash("isSwingingToolUp");
        isSwingingToolDown = Animator.StringToHash("isSwingingToolDown");
        isPickingRight = Animator.StringToHash("isPickingRight");
        isPickingLeft = Animator.StringToHash("isPickingLeft");
        isPickingUp = Animator.StringToHash("isPickingUp");
        isPickingDown = Animator.StringToHash("isPickingDown");

        //Shared Animation parameters
        idleUp = Animator.StringToHash("idleUp");
        idleDown = Animator.StringToHash("idleDown");
        idleLeft = Animator.StringToHash("idleLeft");
        idleRight = Animator.StringToHash("idleRight");
    }
}