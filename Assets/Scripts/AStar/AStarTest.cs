using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;


//[RequireComponent(typeof(AStar))]
public class AStarTest : MonoBehaviour
{
    /*private AStar aStar;
    [SerializeField] private Vector2Int startPosition;
    [SerializeField] private Vector2Int finishPosition;
    [SerializeField] private Tilemap tileMapToDisplayPathOn= null;
    [SerializeField] private TileBase tileMapUseToDisplayPath= null;
    [SerializeField] private bool displayStartAndFinsh= false;
    [SerializeField] private bool displayPath= false;

    private Stack<NPCMovementStep> npcMovementSteps;

    private void Awake()
    {
        aStar = GetComponent<AStar>();
        npcMovementSteps = new Stack<NPCMovementStep>();
    }

    // Update is called once per frame
    void Update()
    {
        if (startPosition!=null && finishPosition !=null && tileMapToDisplayPathOn != null && tileMapUseToDisplayPath !=null)
        {
            //显示路径开始和结束的瓦片
            if (displayStartAndFinsh)
            {
                //显示开始瓦片
                tileMapToDisplayPathOn.SetTile(new Vector3Int(startPosition.x,startPosition.y,0),tileMapUseToDisplayPath);
                //显示结束瓦片
                tileMapToDisplayPathOn.SetTile(new Vector3Int(finishPosition.x,finishPosition.y,0),tileMapUseToDisplayPath);
            }
            //清除瓦片
            else
            {
                //清除开始
                tileMapToDisplayPathOn.SetTile(new Vector3Int(startPosition.x,startPosition.y,0),null);
                //清除结束
                tileMapToDisplayPathOn.SetTile(new Vector3Int(finishPosition.x,finishPosition.y,0),null);
            }

            //显示路径
            if (displayPath)
            {
                //获取当前场景名
                Enum.TryParse<SceneName>(SceneManager.GetActiveScene().name, out SceneName sceneName);

                //建立路径
                aStar.BuildPath(sceneName, startPosition, finishPosition, npcMovementSteps);

                //显示路径在瓦片上
                foreach (NPCMovementStep npcMovementStep in npcMovementSteps)
                {
                    tileMapToDisplayPathOn.SetTile(new Vector3Int(npcMovementStep.gridCoordinate.x,npcMovementStep.gridCoordinate.y,0),tileMapUseToDisplayPath);
                }
            }
            else
            {
                //不显示就清除
                if (npcMovementSteps.Count>0)
                {
                    //清除路径
                    foreach (NPCMovementStep npcMovementStep in npcMovementSteps)
                    {
                        tileMapToDisplayPathOn.SetTile(new Vector3Int(npcMovementStep.gridCoordinate.x,npcMovementStep.gridCoordinate.y,0),null);
                    }

                    //清除堆栈
                    npcMovementSteps.Clear();
                }
            }
        }
    }*/

    [SerializeField] private NPCPath npcPath = null;
    [SerializeField] private bool moveNPC = false;
    [SerializeField] private Vector2Int finishPosition;
    [SerializeField] private AnimationClip idleDownAnimationClip = null;
    [SerializeField] private AnimationClip eventAnimationClip = null;
    private NPCMovement npcMovement;

    private void Start()
    {
        npcMovement = npcPath.GetComponent<NPCMovement>();
        npcMovement.npcFacingDirectionAtDestination = Direction.down;
        npcMovement.npcTargetAnimationClip = idleDownAnimationClip;
    }

    private void Update()
    {
        if (moveNPC)
        {
            moveNPC = false;
            NPCScheduleEvent npcScheduleEvent = new NPCScheduleEvent(0, 0, 0, 0, Weather.none, Season.none, SceneName.Scene1_Farm,
                new GridCoordinate(finishPosition.x, finishPosition.y), eventAnimationClip);

            npcPath.BuildPath(npcScheduleEvent);
        }
    }
}
