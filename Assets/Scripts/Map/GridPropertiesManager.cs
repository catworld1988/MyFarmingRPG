using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(GenerateGUID))]
public class GridPropertiesManager : SingletonMonobehaviour<GridPropertiesManager>, ISaveable
{
    public Grid grid;

    //创建网格属性细节字典
    private Dictionary<string, GridPropertyDetails> gridPropertyDictionary;

    //引入网格属性的 ScriptableObject
    [SerializeField] private SO_GridProperties[] so_gridPropertiesArray = null;

    private string _iSaveableUniqueID;

    public string ISaveableUniqueID
    { get { return _iSaveableUniqueID; }
      set { _iSaveableUniqueID = value; } }


    private GameObjectSave _gameObjectSave;

    public GameObjectSave GameObjectSave
    { get { return _gameObjectSave; }
      set { _gameObjectSave = value; } }


    protected override void Awake()
    {
        base.Awake();

        //获取唯一标识
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave(); //初始化 分配一块内存
    }

    private void OnEnable()
    {
        ISaveableRegister(); //注册 加入保存列表
        EventHandler.AfterSceneLoadEvent += AfterSceneLoaded;
    }

    private void OnDisable()
    {
        ISaveableDeregister();
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoaded;
    }


    private void Start()
    {
        InitialiseGridProperties();
    }

    private void InitialiseGridProperties()
    {
        //遍历所有网格属性
        foreach (SO_GridProperties so_GridProperties in so_gridPropertiesArray)
        {
            //创建字典存储属性细节
            Dictionary<string, GridPropertyDetails> gridPropertyDictionary = new Dictionary<string, GridPropertyDetails>();

            //填充属性细节字典
            foreach (GridProperty gridProperty in so_GridProperties.GridPropertyList)
            {
                // ReSharper disable once JoinDeclarationAndInitializer
                GridPropertyDetails gridPropertyDetails;

                gridPropertyDetails = GetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDictionary);

                if (gridPropertyDetails == null)
                {
                    gridPropertyDetails = new GridPropertyDetails();
                }

                switch (gridProperty.gridBoolProperty)
                {
                    case GridBoolProperty.diggable:
                        gridPropertyDetails.isDiggable = gridProperty.gridBoolValue;
                        break;

                    case GridBoolProperty.canDropItem:
                        gridPropertyDetails.canDropItem = gridProperty.gridBoolValue;
                        break;

                    case GridBoolProperty.canPlaceFurniture:
                        gridPropertyDetails.canPlaceFurniture = gridProperty.gridBoolValue;
                        break;

                    case GridBoolProperty.isPath:
                        gridPropertyDetails.isPath = gridProperty.gridBoolValue;
                        break;

                    case GridBoolProperty.isNPCObstacle:
                        gridPropertyDetails.isNPCObstacle = gridProperty.gridBoolValue;
                        break;

                    default:
                        break;
                }

                SetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDetails, gridPropertyDictionary);
            }

            //创建新的场景数据
            SceneSave sceneSave = new SceneSave();

            //将网格属性字典添加到 场景数据中
            sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;

            //将 匹配场景 的字典数据填充进去
            if (so_GridProperties.sceneName.ToString()== SceneControllerManager.Instance.startingSceneName.ToString())
            {
                this.gridPropertyDictionary = gridPropertyDictionary;
            }
            //将场景数据 加入对象数据的场景数据字典里
            GameObjectSave.sceneDate.Add(so_GridProperties.sceneName.ToString(),sceneSave);
        }
    }

    /// <summary>
    /// Set the grid property details to gridPropertyDetails for the tile at (gridx,gridy)for the gridpropertyDictionary
    /// 将 gridPropertyDictionary 的网格属性详细信息设置为 gridPropertyDetails (gridx，gridy)
    /// </summary>
    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails, Dictionary<string, GridPropertyDetails>
        gridPropertyDictionary)
    {
        string key = "x" + gridX + "y" + gridY;

        gridPropertyDetails.gridX = gridX;
        gridPropertyDetails.gridY = gridY;

        gridPropertyDictionary[key] = gridPropertyDetails;

    }
    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails)
    {
        string key = "x" + gridX + "y" + gridY;

        gridPropertyDetails.gridX = gridX;
        gridPropertyDetails.gridY = gridY;

        gridPropertyDictionary[key] = gridPropertyDetails;

    }

    private void AfterSceneLoaded()
    {
        //加载后 获得网格
        grid = GameObject.FindObjectOfType<Grid>();
    }


    /// <summary>
    /// Returns the gridPropertyDetails at the gridlocation for the supplied dictionary,or null if no properties exist at that location.
    ///  返回所提供词典的网格位置处的gridPropertyDetail，如果该位置不存在任何属性，则返回NULL。
    /// </summary>
    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY, Dictionary<string, GridPropertyDetails> gridPropertyDictionary)
    {
        //Construct key from coordinate
        //组合一个键值
        string key = "x" + gridX + "y" + gridY;

        GridPropertyDetails gridPropertyDetails;

        //Check if grid property details exist for coordinate and retrieve
        //检查是否存在坐标和检索的网格属性详细信息
        if (!gridPropertyDictionary.TryGetValue(key, out gridPropertyDetails))
        {
            //if not found
            return null;
        }
        else
        {
            return gridPropertyDetails;
        }
    }

    /// <summary>
    /// Get the grid property details for the tile at (gridx,gridY).If no grid property details exist null is returned
    /// and can assume that all grid propertydetails values are null or false
    /// 在（gridx、gridY）处获取瓷砖的网格属性详细信息。如果不存在网格属性详细信息，则返回null，并且可以假定所有网格属性详细信息值都为空或假
    /// </summary>
    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY)
    {
        return GetGridPropertyDetails(gridX, gridY, gridPropertyDictionary);
    }


    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public GridPropertyDetails gridPropertyDetails(int gridX, int gridY)
    {
        return GetGridPropertyDetails(gridX, gridY, gridPropertyDictionary);
    }

    /// <summary>
    /// 将网格属性细节 保存场景数据
    /// </summary>
    public void ISaveableStoreScene(string sceneName)
    {
        //清除原有数据
        GameObjectSave.sceneDate.Remove(sceneName);

        SceneSave sceneSave = new();

        //填充新的网格属性字典
        sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;

        //更新到对象保存数据
        GameObjectSave.sceneDate.Add(sceneName,sceneSave);
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        //尝试加载数据
        if (GameObjectSave.sceneDate.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            //检测网格属性细节字典是否存在
            if (sceneSave.gridPropertyDetailsDictionary != null)
            {
                gridPropertyDictionary = sceneSave.gridPropertyDetailsDictionary; //导出数据
            }
        }
    }
}