using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NPCPath))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class NPCMovement : MonoBehaviour
{
    public SceneName npcCurrentScene;
    [HideInInspector] public SceneName npcTargetScene;
    [HideInInspector] public Vector3Int npcCurrentGridPosition;
    [HideInInspector] public Vector3Int npcTargetGridPosition;
    [HideInInspector] public Vector3 npcTargetWorldPosition;
    public Direction npcFacingDirectionAtDestination;

    private SceneName npcPreviousMovementStepScene;
    private Vector3Int npcNextGridPosition;
    private Vector3 npcNextWorldPosition;

    [Header("NPC Movement")]
    public float npcNormalSpeed = 2f;

    [SerializeField] private float npcMinSpeed = 1f;
    [SerializeField] private float npcMaxSpeed = 3f;
    private bool npcIsMoving = false;

    [HideInInspector] public AnimationClip npcTargetAnimationClip;

    [Header("NPC Animation")]
    [SerializeField] private AnimationClip blankAnimation = null;

    private Grid grid;
    private Rigidbody2D rigidBody2D;
    private BoxCollider2D boxCollider2D;
    private WaitForFixedUpdate waitForFixedUpdate;
    private Animator animator;
    private AnimatorOverrideController animatorOverrideController;
    private int lastMoveAnimationParameter;
    private NPCPath npcPath;
    private bool npcInitialised = false;
    private SpriteRenderer spriteRenderer;
    [HideInInspector] public bool npcActiveInScene = false;

    private bool sceneLoaded = false;

    private Coroutine moveToGridPositionRoutine;

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnloaded;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnloaded;
    }

    private void Awake()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        npcPath = GetComponent<NPCPath>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        //Initialise target world position,target grid position target scene to current 初始化目标的世界地位,当前目标网格位置目标场景
        npcTargetScene = npcCurrentScene;
        npcTargetGridPosition = npcCurrentGridPosition;
        npcTargetWorldPosition = transform.position;
    }

    private void Start()
    {
        waitForFixedUpdate = new WaitForFixedUpdate();

        SetIdleAnimation();
    }

    private void FixedUpdate()
    {
        if (sceneLoaded)
        {
            if (npcIsMoving==false)
            {
                //set npc current and next grid position to take into account the npc might be animating 设置npc当前个下个网格位置
                npcCurrentGridPosition = GetGridPosition(transform.position);
                npcNextGridPosition = npcCurrentGridPosition;

                if (npcPath.npcMovementStepStack.Count>0)
                {
                    NPCMovementStep npcMovementStep = npcPath.npcMovementStepStack.Peek();

                    npcCurrentScene = npcMovementStep.sceneName;

                    //If NPC is about the move to a new scene reset position to starting point in new scene and update the step times
                    //如果NPC要移动到新场景，则将位置重置到新场景的起点，并更新步骤时间
                    if (npcCurrentScene!=npcPreviousMovementStepScene)
                    {
                        npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                        npcNextGridPosition = npcCurrentGridPosition;
                        transform.position = GetWorldPosition(npcCurrentGridPosition);
                        npcPreviousMovementStepScene = npcCurrentScene;
                        npcPath.UpdateTimesOnPath();
                    }

                    //If NPC is in current scene then set NPC to active to make visible,pop the movement step off the stack and then call method to move NPC
                    //如果NPC在当前场景中，则将NPC设置为活动以使其可见，从堆栈中弹出移动步骤，然后调用方法来移动NPC
                    if (npcCurrentScene.ToString() == SceneManager.GetActiveScene().name)
                    {
                        SetNPCActiveInScene();

                        npcMovementStep = npcPath.npcMovementStepStack.Pop();

                        npcNextGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;

                        TimeSpan npcMovementStepTime = new TimeSpan(npcMovementStep.hour, npcMovementStep.minute, npcMovementStep.second);

                        MoveToGridPosition(npcNextGridPosition, npcMovementStepTime, TimeManager.Instance.GetGameTime());
                    }

                    //else if NPC is not in current scene then set NPC to inactive to make invisible 否则，如果 NPC 不在当前场景中，则将 NPC 设置为非活动状态以使其不可见
                    //once the movement step time is less than game time (in the past)then pop movement step off the stack and set NPC position to movement step position
                    //一旦移动步骤时间小于游戏时间(在过去) ，则弹出移动步骤从堆栈中移出，并将 NPC 位置设置为移动步骤位置
                    else
                    {
                        SetNPCInactiveInScene();

                        npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                        npcNextGridPosition = npcCurrentGridPosition;
                        transform.position = GetWorldPosition(npcCurrentGridPosition);

                        TimeSpan npcMovementStepTime = new TimeSpan(npcMovementStep.hour, npcMovementStep.minute, npcMovementStep.second);

                        TimeSpan gameTime = TimeManager.Instance.GetGameTime();

                        if (npcMovementStepTime<gameTime)
                        {
                            npcMovementStep = npcPath.npcMovementStepStack.Pop();

                            npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                            npcNextGridPosition = npcCurrentGridPosition;
                            transform.position = GetWorldPosition(npcCurrentGridPosition);
                        }
                    }
                }
                //否则，如果没有更多的NPC运动步骤
                else
                {
                    ResetMoveAnimation();

                    SetNPCFacingDirection();

                    SetNPCEventAnimation();
                }
            }
        }
    }

    public void SetScheduleEventDetails(NPCScheduleEvent npcScheduleEvent)
    {
        npcTargetScene = npcScheduleEvent.toSceneName;
        npcTargetGridPosition = (Vector3Int)npcScheduleEvent.toGridCoordinate;
        npcTargetWorldPosition = GetWorldPosition(npcTargetGridPosition);
        npcFacingDirectionAtDestination = npcScheduleEvent.npcFacingDirectionAtDestination;
        npcTargetAnimationClip = npcScheduleEvent.animationAtDestination;

        ClearNPCEventAnimation();
    }

    private void SetNPCEventAnimation()
    {
        if (npcTargetAnimationClip !=null)
        {
            ResetIdleAnimation();
            animatorOverrideController[blankAnimation] = npcTargetAnimationClip;
            animator.SetBool(Settings.eventAnimation,true);
        }
        else
        {
            animatorOverrideController[blankAnimation] = blankAnimation;
            animator.SetBool(Settings.eventAnimation,false);
        }

    }
    public void ClearNPCEventAnimation()
    {
        animatorOverrideController[blankAnimation] = blankAnimation;
        animator.SetBool(Settings.eventAnimation,false);

        //清除NPC旋转
        transform.rotation = Quaternion.identity;
    }
    private void SetNPCFacingDirection()
    {
        ResetIdleAnimation();

        switch (npcFacingDirectionAtDestination)
        {
            case Direction.up:
                animator.SetBool(Settings.idleUp,true);
                break;
            case Direction.down:
                animator.SetBool(Settings.idleDown,true);
                break;
            case Direction.left:
                animator.SetBool(Settings.idleLeft,true);
                break;
            case Direction.right:
                animator.SetBool(Settings.idleRight,true);
                break;
            case  Direction.none:
                break;
            default:
                break;
        }
    }

    public void SetNPCActiveInScene()
    {
        spriteRenderer.enabled = true;
        boxCollider2D.enabled = true;
        npcActiveInScene = true;
    }

    public void SetNPCInactiveInScene()
    {
        spriteRenderer.enabled = false;
        boxCollider2D.enabled = false;
        npcActiveInScene = false;
    }

    private void AfterSceneLoad()
    {
        grid = GameObject.FindObjectOfType<Grid>();

        if (!npcInitialised)
        {
            InitialiseNPC();
            npcInitialised = true;
        }

        sceneLoaded = true;
    }

    private void BeforeSceneUnloaded()
    {
        sceneLoaded = false;
    }

    /// <summary>
    /// 根据世界坐标返回网格位置
    /// </summary>
    private Vector3Int GetGridPosition(Vector3 worldPosition)
    {
        if (grid!=null)
        {
            return grid.WorldToCell(worldPosition);
        }
        else
        {
            return Vector3Int.zero;
        }
    }

    /// <summary>
    /// 根据网格位置 获取世界坐标
    /// </summary>
    public Vector3 GetWorldPosition(Vector3Int gridPosition)
    {
        Vector3 worldPosition = grid.CellToWorld(gridPosition);

        return new Vector3(worldPosition.x + Settings.gridCellSize / 2f, worldPosition.y + Settings.gridCellSize / 2f, worldPosition.z);
    }

    public void CancelNPCMovement()
    {
        npcPath.ClearPath();
        npcNextGridPosition= Vector3Int.zero;
        npcNextWorldPosition = Vector3.zero;
        npcIsMoving = false;

        if (moveToGridPositionRoutine != null)
        {
            StopCoroutine(moveToGridPositionRoutine);
        }

        //重置移动动画
        ResetMoveAnimation();

        //取消事件动画
        ClearNPCEventAnimation();
        npcTargetAnimationClip = null;

        //重置闲置动画
        ResetIdleAnimation();

        //设置闲置动画
        SetIdleAnimation();
    }

    private void InitialiseNPC()
    {
        //在场景激活
        if (npcCurrentScene.ToString() == SceneManager.GetActiveScene().name)
        {
            SetNPCActiveInScene();
        }
        else
        {
            SetNPCInactiveInScene();
        }

        npcPreviousMovementStepScene = npcCurrentScene;

        npcCurrentGridPosition = GetGridPosition(transform.position);

        //设置下一个网格位置 和目标网格位置 到当前的网格位置
        npcNextGridPosition = npcCurrentGridPosition;
        npcTargetGridPosition = npcCurrentGridPosition;
        npcTargetWorldPosition = GetWorldPosition(npcTargetGridPosition);

        //获得npc世界位置
        npcNextWorldPosition = GetWorldPosition(npcCurrentGridPosition);
    }




    private void MoveToGridPosition(Vector3Int gridPosition, TimeSpan npcMovementStepTime, TimeSpan gameTime)
    {
        moveToGridPositionRoutine = StartCoroutine(MoveToGridPositionRoutine(gridPosition, npcMovementStepTime, gameTime));
    }

    private IEnumerator MoveToGridPositionRoutine(Vector3Int gridPosition, TimeSpan npcMovementStepTime, TimeSpan gameTime)
    {
        npcIsMoving = true;

        SetMoveAnimation(gridPosition);

        npcNextWorldPosition = GetWorldPosition(gridPosition);

        //If movement step time is in the future,otherwise skip and move NPC immediately to position
        //如果移动步长时间在将来，则跳过并立即将NPC移动到位置
        if (npcMovementStepTime>gameTime)
        {
            //计算时间差，单位为秒
            float timeToMove = (float)(npcMovementStepTime.TotalSeconds - gameTime.TotalSeconds);

            //npc计算速度 速度太低npc会跳跃所以选择最大速度
            float npcCalculatedSpeed = Mathf.Max(npcMinSpeed,Vector3.Distance(transform.position, npcNextWorldPosition) / timeToMove / Settings.secondsPerGameSecond) ;

            //If speed is less than npc max speed then process,otherwise skip and move NPC immediately to position
            //如果速度小于NPC最大速度，则处理，否则跳过并立即将NPC移动到位置
            if (npcCalculatedSpeed<=npcMaxSpeed)
            {
                while (Vector3.Distance(transform.position,npcNextWorldPosition) > Settings.pixelSize)
                {
                    Vector3 unitVector= Vector3.Normalize(npcNextWorldPosition - transform.position);
                    Vector2 move = new Vector2(unitVector.x * npcCalculatedSpeed * Time.fixedDeltaTime, unitVector.y * npcCalculatedSpeed * Time.fixedDeltaTime);

                    rigidBody2D.MovePosition(rigidBody2D.position + move);

                    yield return waitForFixedUpdate;
                }
            }
        }

        rigidBody2D.position = npcNextWorldPosition;
        npcCurrentGridPosition = gridPosition;
        npcNextGridPosition = npcCurrentGridPosition;
        npcIsMoving = false;
    }



    private void SetMoveAnimation(Vector3Int gridPosition)
    {
        //重置站立动画
        ResetIdleAnimation();

        //重置移动动画
        ResetMoveAnimation();

        //获得世界位置
        Vector3 toWorldPosition = GetWorldPosition(gridPosition);

        //get Vector 获得方向向量
        Vector3 directionVector = toWorldPosition - transform.position;

        if (Mathf.Abs(directionVector.x)>= Mathf.Abs(directionVector.y))
        {
            //使用 左/右 走动画
            if (directionVector.x>0)
            {
                animator.SetBool(Settings.walkRight,true);
            }
            else
            {
                animator.SetBool(Settings.walkLeft,true);
            }
        }
        else
        {
            //使用 上/下 走动画
            if (directionVector.y>0)
            {
                animator.SetBool(Settings.walkUp,true);
            }
            else
            {
                animator.SetBool(Settings.walkDown,true);
            }
        }
    }

    private void SetIdleAnimation()
    {
        animator.SetBool(Settings.idleDown,true);
    }


    private void ResetMoveAnimation()
    {
        animator.SetBool(Settings.walkRight,false);
        animator.SetBool(Settings.walkLeft,false);
        animator.SetBool(Settings.walkUp,false);
        animator.SetBool(Settings.walkDown,false);
    }

    private void ResetIdleAnimation()
    {
        animator.SetBool(Settings.idleRight,false);
        animator.SetBool(Settings.idleLeft,false);
        animator.SetBool(Settings.idleUp,false);
        animator.SetBool(Settings.idleDown,false);
    }
}
