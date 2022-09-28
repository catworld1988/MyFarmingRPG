using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCMovement))]
[RequireComponent(typeof(GenerateGUID))]
public class NPC : MonoBehaviour,ISaveable
{
    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set {  _gameObjectSave = value; } }

    private NPCMovement npcMovement;

    private void OnEnable()
    {
        ISaveableRegister();
    }

    private void OnDisable()
    {
        ISaveableDeregister();
    }

    private void Awake()
    {
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void Start()
    {
        npcMovement = GetComponent<NPCMovement>();
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
        //清除当前场景数据
        GameObjectSave.sceneDate.Remove(Settings.PersistentScene);

        //初始化新的场景保存
        SceneSave sceneSave = new SceneSave();

        sceneSave.vector3Dictionary = new Dictionary<string, Vector3Serializable>();
        sceneSave.stringDictionary = new Dictionary<string, string>();

        //存储可序列化 目标网格位置 目标世界位置 目标场景
        sceneSave.vector3Dictionary.Add("npcTargetGridPosition",new Vector3Serializable(npcMovement.npcTargetGridPosition.x,npcMovement.npcTargetGridPosition.y,npcMovement.npcTargetGridPosition.z));
        sceneSave.vector3Dictionary.Add("npcTargetWorldPosition",new Vector3Serializable(npcMovement.npcTargetWorldPosition.x,npcMovement.npcTargetWorldPosition.y,npcMovement.npcTargetWorldPosition.z));
        sceneSave.stringDictionary.Add("npcTargetScene",npcMovement.npcTargetScene.ToString());

        //添加场景保存到 游戏保存对象中
        GameObjectSave.sceneDate.Add(Settings.PersistentScene,sceneSave);
        return GameObjectSave;
    }


    public void ISaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID,out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;

            //获取scene save
            if (GameObjectSave.sceneDate.TryGetValue(Settings.PersistentScene,out SceneSave sceneSave))
            {
                //如果存在字典 提取位置和场景
                if (sceneSave.vector3Dictionary!=null && sceneSave.stringDictionary !=null)
                {
                    //目标网格位置
                    if (sceneSave.vector3Dictionary.TryGetValue("npcTargetGridPosition",out Vector3Serializable savedNPCTargetGridPosition))
                    {
                        npcMovement.npcTargetGridPosition = new Vector3Int((int)savedNPCTargetGridPosition.x, (int)savedNPCTargetGridPosition.y,
                            (int)savedNPCTargetGridPosition.z);
                        npcMovement.npcCurrentGridPosition = npcMovement.npcTargetGridPosition;
                    }

                    //目标世界位置
                    if (sceneSave.vector3Dictionary.TryGetValue("npcTargetWorldPosition",out Vector3Serializable savedNPCTargetWorldPosition))
                    {
                        npcMovement.npcTargetWorldPosition =
                            new Vector3(savedNPCTargetWorldPosition.x, savedNPCTargetWorldPosition.y, savedNPCTargetWorldPosition.z);
                        transform.position = npcMovement.npcTargetWorldPosition;
                    }

                    //目标场景
                    if (sceneSave.stringDictionary.TryGetValue("npcTargetScene",out string saveTargetScene))
                    {
                        if (Enum.TryParse<SceneName>(saveTargetScene,out SceneName sceneName))
                        {
                            npcMovement.npcTargetScene = sceneName;
                            npcMovement.npcCurrentScene = npcMovement.npcTargetScene;
                        }
                    }

                    //取消任何当前npc移动
                    npcMovement.CancelNPCMovement();
                }
            }
        }
    }



    public void ISaveableStoreScene(string sceneName)
    {
        //这里没有任何要求，因为在持久的场景中
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        //这里没有任何要求，因为在持久的场景中
    }
}

