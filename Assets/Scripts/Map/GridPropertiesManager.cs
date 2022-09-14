using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(GenerateGUID))]
public class GridPropertiesManager : SingletonMonobehaviour<GridPropertiesManager>, ISaveable
{
    //储存网格的变量
    private Tilemap groundDecoration1;
    private Tilemap groundDecoration2;

    private Grid grid;

    //创建网格属性细节字典
    private Dictionary<string, GridPropertyDetails> gridPropertyDictionary;

    //引入网格属性的 ScriptableObject
    [SerializeField] private SO_GridProperties[] so_gridPropertiesArray = null;

    //获得需要挖掘的地面瓷砖  放在一个数组
    [SerializeField] private Tile[] dugGround = null;


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

    private void ClearDisplayGroundDecorations()
    {
        //清除所有的瓦片

        groundDecoration1.ClearAllTiles();
        groundDecoration2.ClearAllTiles();
    }

    private void ClearDisplayGridPropertyDetails()
    {
        ClearDisplayGroundDecorations();
    }

    public void DisplayDugGround(GridPropertyDetails gridPropertyDetails)
    {
        //> -1 代表被挖了
        if (gridPropertyDetails.daysSinceDug>-1)
        {
            ConnectDugGround(gridPropertyDetails);
        }
    }

    /// <summary>
    /// 设置相邻的 4片瓦片地图块
    /// </summary>
    /// <param name="gridPropertyDetails"></param>
    private void ConnectDugGround(GridPropertyDetails gridPropertyDetails)
    {

        //选择的基础瓦片
        Tile dugTile0 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX,gridPropertyDetails.gridY,0),dugTile0);

        GridPropertyDetails adjacentGidGridPropertyDetails;  //相邻瓦片属性详情

        //-----------------------------Set 4 tiles if dug surrounding current tile up ,down， left ,right   now that this central tile has been dug
        //设置4个瓷砖.如果挖掘当前瓷砖 上，下，左，右。现在这个中心瓷砖已经挖好。


        //up的瓦片
        adjacentGidGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        //相邻存在瓦片并且被挖过
        if (adjacentGidGridPropertyDetails!=null&& adjacentGidGridPropertyDetails.daysSinceDug>-1)
        {
            Tile dugTile1=SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY+1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX,gridPropertyDetails.gridY+1,0),dugTile1);

        }

        //down的瓦片
        adjacentGidGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        //相邻存在瓦片并且被挖过
        if (adjacentGidGridPropertyDetails!=null&& adjacentGidGridPropertyDetails.daysSinceDug>-1)
        {
            Tile dugTile2=SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY-1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX,gridPropertyDetails.gridY-1,0),dugTile2);

        }

        //left的瓦片
        adjacentGidGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX-1, gridPropertyDetails.gridY);
        //相邻存在瓦片并且被挖过
        if (adjacentGidGridPropertyDetails!=null&& adjacentGidGridPropertyDetails.daysSinceDug>-1)
        {
            Tile dugTile3=SetDugTile(gridPropertyDetails.gridX-1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX-1,gridPropertyDetails.gridY,0),dugTile3);

        }

        //right的瓦片
        adjacentGidGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX+1, gridPropertyDetails.gridY);
        //相邻存在瓦片并且被挖过
        if (adjacentGidGridPropertyDetails!=null&& adjacentGidGridPropertyDetails.daysSinceDug>-1)
        {
            Tile dugTile4=SetDugTile(gridPropertyDetails.gridX+1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX+1,gridPropertyDetails.gridY,0),dugTile4);

        }
    }

    /// <summary>
    /// 设置被挖的瓦片
    /// </summary>
    private Tile SetDugTile(int xGrid, int yGrid)
    {
        bool upDug= IsGridSquareDug(xGrid,yGrid+1);
        bool downDug= IsGridSquareDug(xGrid,yGrid-1);
        bool leftDug= IsGridSquareDug(xGrid-1,yGrid);
        bool rightDug= IsGridSquareDug(xGrid+1,yGrid);


        #region set appropriate tile based once whether surrounding tiles are dug or not

        if (!upDug&& !downDug && !rightDug && !leftDug)
        {
            return dugGround[0];
        }
        else if (!upDug&& downDug && rightDug && !leftDug)
        {
            return dugGround[1];
        }
        else if (!upDug&& downDug && rightDug && leftDug)
        {
            return dugGround[2];
        }
        else if (!upDug&& downDug && !rightDug && leftDug)
        {
            return dugGround[3];
        }
        else if (!upDug&& downDug && !rightDug && !leftDug)
        {
            return dugGround[4];
        }
        else if (upDug&& downDug && rightDug && !leftDug)
        {
            return dugGround[5];
        }
        else if (upDug&& downDug && rightDug && leftDug)
        {
            return dugGround[6];
        }
        else if (upDug&& downDug && !rightDug && leftDug)
        {
            return dugGround[7];
        }
        else if (upDug&& downDug && !rightDug && !leftDug)
        {
            return dugGround[8];
        }
        else if (upDug&& !downDug && rightDug && !leftDug)
        {
            return dugGround[9];
        }
        else if (upDug&& !downDug && rightDug && leftDug)
        {
            return dugGround[10];
        }
        else if (upDug&& !downDug && !rightDug && leftDug)
        {
            return dugGround[11];
        }
        else if (upDug&& !downDug && !rightDug && !leftDug)
        {
            return dugGround[12];
        }
        else if (!upDug&& !downDug && rightDug && !leftDug)
        {
            return dugGround[13];
        }
        else if (!upDug&& !downDug && rightDug && leftDug)
        {
            return dugGround[14];
        }
        else if (!upDug&& !downDug && !rightDug && leftDug)
        {
            return dugGround[15];
        }

        return null;


        #endregion Set appropriate tile based on whether surrounding tiles are dug or not 根据周围的瓦片是否被挖出来设置适当的切片
    }


    ///判断地块是否被挖了
    private bool IsGridSquareDug(int xGrid, int yGrid)
    {
        GridPropertyDetails gridPropertyDetails=GetGridPropertyDetails(xGrid, yGrid);

        if (gridPropertyDetails==null)
        {
            return false;
        }
        else if (gridPropertyDetails.daysSinceDug > -1)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    /// <summary>
    /// 显示所地面的属性
    /// </summary>
    private void DisplayGridPropertyDetails()
    {
        //循环所有的瓦片
        foreach (KeyValuePair<string, GridPropertyDetails> item in gridPropertyDictionary)
        {
            GridPropertyDetails gridPropertyDetails= item.Value;
            DisplayDugGround(gridPropertyDetails);
        }
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

        groundDecoration1= GameObject.FindGameObjectWithTag(Tags.GroundDecoration1).GetComponent<Tilemap>();
        groundDecoration2= GameObject.FindGameObjectWithTag(Tags.GroundDecoration2).GetComponent<Tilemap>();
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

            //加载地图上面的属性格子 如果网格 属性存在
            if (gridPropertyDictionary.Count > 0)
            {
                //grid property details found for the current scene destroy existing ground decoration 清除当前场景中的瓦片 地面属性装饰
                ClearDisplayGridPropertyDetails();

                //显示地面被挖的属性
                DisplayGridPropertyDetails();
            }

        }
    }
}