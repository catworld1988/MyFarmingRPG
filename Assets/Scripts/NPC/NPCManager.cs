using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(AStar))]
public class NPCManager : SingletonMonobehaviour<NPCManager>
{
    //npc寻路变量
    [SerializeField] private SO_SceneRouteList so_SceneRouteList = null;
    private Dictionary<string, SceneRoute> sceneRouteDictionary;

    [HideInInspector]
    public NPC[] npcArray;

    private AStar aStar;

    protected override void Awake()
    {
        base.Awake();

        //创建场景路线字典
        sceneRouteDictionary = new Dictionary<string, SceneRoute>();

        if (so_SceneRouteList.sceneRouteList.Count>0)
        {
            //在so路线列表中检索路线
            foreach (SceneRoute so_sceneRoute in so_SceneRouteList.sceneRouteList)
            {
                //检测相同重复路线
                if (sceneRouteDictionary.ContainsKey(so_sceneRoute.fromScenenName.ToString() + so_sceneRoute.toScenenName.ToString()))
                {
                    Debug.Log("** 检索到重复相同路线的键值冲突 ** 在scriptable object scene route list 中检查重复的路线");
                    continue;
                }
                //添加路线到字典
                sceneRouteDictionary.Add(so_sceneRoute.fromScenenName.ToString() + so_sceneRoute.toScenenName.ToString(),so_sceneRoute);
            }
        }


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

    /// <summary>
    /// npc激活状态
    /// </summary>
    private void SetNPCsActiveStatus()
    {
        foreach (NPC npc in npcArray)
        {
            NPCMovement npcMovement = npc.GetComponent<NPCMovement>();

            if (npcMovement.npcCurrentScene.ToString()== SceneManager.GetActiveScene().name)
            {
                npcMovement.SetNPCActiveInScene();
            }
            else
            {
                npcMovement.SetNPCInactiveInScene();
            }
        }
    }

    public SceneRoute GetSceneRoute(string fromSceneName, string toSceneName)
    {
        SceneRoute sceneRoute;

        //用场景名字从字典获取路线
        if (sceneRouteDictionary.TryGetValue(fromSceneName+toSceneName,out sceneRoute))
        {
            return sceneRoute;
        }
        else
        {
            return null;
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
