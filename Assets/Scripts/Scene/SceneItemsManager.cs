using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GenerateGUID))]
public class SceneItemsManager : SingletonMonobehaviour<SceneItemsManager>, ISaveable
{
    private Transform parentItem; // 物品的父级 便于存放实例化的物品

    [SerializeField] private GameObject itemPrefab = null;   //物品预制件 之后用来实例化重建物品


    //唯一ID
    private string _iSaveableUniqueID;

    public string ISaveableUniqueID
    {
        get => _iSaveableUniqueID;
        set => _iSaveableUniqueID = value;
    }


    private GameObjectSave _gameObjectSave;

    public GameObjectSave GameObjectSave
    {
        get { return _gameObjectSave; }
        set { _gameObjectSave = value; }
    }

    //收到场景订阅通知 执行找父级的方法
    private void AfterSceneLoad()
    {
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
    }

    protected override void Awake()
    {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }


    /// <summary>
    /// 销毁场景中所有的物品
    /// </summary>
    private void DestorySceneItems()
    {
        //获得场景中所有物品
        Item[] itemsInScene = GameObject.FindObjectsOfType<Item>();

        //遍历所有物品 并销毁
        for (int i = itemsInScene.Length - 1; i > -1; i--)
        {
            Destroy(itemsInScene[i].gameObject);
        }
    }

    /// <summary>
    /// 实例化 单独物件
    /// </summary>
    public void InstantiateSceneItem(int itemCode, Vector3 itemPosition)
    {
        GameObject itemGameObject = Instantiate(itemPrefab, itemPosition, Quaternion.identity, parentItem);
        Item item = itemGameObject.GetComponent<Item>();

        //给物件添加碰撞晃动
        item.Init(itemCode);
    }

    /// <summary>
    /// 根据存储的数据 实例化物件
    /// </summary>
    public void InstantiateSceneItem(List<SceneItem> sceneItemList)
    {
        GameObject itemGameObject;

        foreach (SceneItem sceneItem in sceneItemList)
        {
            //实例化
            itemGameObject = Instantiate(itemPrefab, new Vector3(sceneItem.position.x, sceneItem.position.y, sceneItem.position.z),
                Quaternion.identity, parentItem);

            //设置组件 填充参数
            Item item = itemGameObject.GetComponent<Item>();
            item.ItemCode = sceneItem.itemCode;
            item.name = sceneItem.itemName;
        }
    }

    private void OnEnable()
    {
        ISaveableRegister(); //注册 在 接口类型列表
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    private void OnDisable()
    {
        ISaveableDeregister(); //注销 在 接口类型列表
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    public void ISaveableRegister()
    {
        //注册 在 接口类型列表
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        //注销 在 接口类型列表
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public GameObjectSave ISaveableSave()
    {
        ISaveableRestoreScene(SceneManager.GetActiveScene().name);

        return GameObjectSave;
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID,out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;

            ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }

    }

    /// <summary>
    /// 保存场景数据
    /// </summary>
    public void ISaveableStoreScene(string sceneName)
    {
        //去掉旧场景的数据 如果存在的话
        GameObjectSave.sceneDate.Remove(sceneName);

        //获得所有当前场景物件
        List<SceneItem> sceneItemList = new List<SceneItem>();
        Item[] itemsInScene = FindObjectsOfType<Item>();

        //循环将 物件存放在场景物件列表
        foreach (Item item in itemsInScene)
        {
            //将物件   填充为序列化数据 的实例物件
            SceneItem sceneItem = new SceneItem();
            sceneItem.itemCode = item.ItemCode;
            sceneItem.position = new Vector3Serializable(item.transform.position.x, item.transform.position.y, item.transform.position.z);
            sceneItem.itemName = item.name;

            //添加填充好的场景物件们 加到 列表（包含所有已见的物品）
            sceneItemList.Add(sceneItem);
        }

        //创建场景物品字典 添加物件列表
        SceneSave sceneSave = new SceneSave();
        sceneSave.listSceneItem = sceneItemList;

        //将场景数据 添加到对象中
        GameObjectSave.sceneDate.Add(sceneName, sceneSave);
    }


    /// <summary>
    /// 恢复 场景数据
    /// </summary>
    public void ISaveableRestoreScene(string sceneName)
    {
        //检测是否有场景数据 返回场景数据
        if (GameObjectSave.sceneDate.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            //检测是否有物品数据 返回物品列表数据
            if (sceneSave.listSceneItem != null )
            {
                //销毁场景中所有物品
                DestorySceneItems();

                //根据列表数据重建 实例化物品
                InstantiateSceneItem(sceneSave.listSceneItem);
            }
        }
    }
}