using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(AStar))]
public class NPCManager : SingletonMonobehaviour<NPCManager>
{
    [HideInInspector]
    public NPC[] npcArray;

    private AStar aStar;

    protected override void Awake()
    {
        base.Awake();

        aStar = GetComponent<AStar>();

        //在场景中获取npc对象 利用挂载的空的NPC类
        npcArray = FindObjectsOfType<NPC>();
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    private void AfterSceneLoad()
    {
        SetNPCsActiveStatus();
    }

    private void SetNPCsActiveStatus()
    {
        foreach (NPC npc in npcArray)
        {
            NPCMovement npcMovement = npc.GetComponent<NPCMovement>();

            if (npcMovement.npcCurrentScene.ToString()== SceneManager.GetActiveScene().ToString())
            {
                npcMovement.SetNPCActiveInScene();
            }
            else
            {
                npcMovement.SetNPCInactiveInScene();
            }
        }
    }

    /// <summary>
    /// 调佣AStar的BuildPath
    /// </summary>
    public bool BuildPath(SceneName sceneName,Vector2Int startGridPosition,Vector2Int endGridPosition,Stack<NPCMovementStep> npcMovementStepStack)
    {
        if (aStar.BuildPath(sceneName,startGridPosition,endGridPosition,npcMovementStepStack))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
