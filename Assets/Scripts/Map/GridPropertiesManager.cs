using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(GenerateGUID))]
public class GridPropertiesManager : SingletonMonobehaviour<GridPropertiesManager>, ISaveable
{
    //作物的变量
    private Transform cropsParentTransform; //作物的父级
    [SerializeField] public SO_CropDetailsList so_CropDetailsList = null; //作物的数据

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

    //被水浇过的地面
    [SerializeField] private Tile[] wateredGround = null;

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
        ISaveableRegister(); //注册 加入保存数据列表
        EventHandler.AfterSceneLoadEvent += AfterSceneLoaded;

        //游戏里经过一天
        EventHandler.AdvanceGameDayEvent += AdvanceDay;
    }

    private void OnDisable()
    {
        ISaveableDeregister(); //注册 加入恢复数据列表
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoaded;

        //游戏里经过一天
        EventHandler.AdvanceGameDayEvent -= AdvanceDay;
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

    ///清除显示多有种植的农作物
    private void ClearDisplayAllPlantedCrops()
    {
        Crop[] cropArray;
        cropArray = FindObjectsOfType<Crop>();

        foreach (Crop crop in cropArray)
        {
            Destroy(crop.gameObject);
        }
    }

    /// <summary>
    /// 清除网格上的属性细节
    /// </summary>
    private void ClearDisplayGridPropertyDetails()
    {
        ClearDisplayGroundDecorations();


        //清除显示多有种植的农作物
        ClearDisplayAllPlantedCrops();
    }


    public void DisplayDugGround(GridPropertyDetails gridPropertyDetails)
    {
        //> -1 代表被挖了
        if (gridPropertyDetails.daysSinceDug > -1)
        {
            ConnectDugGround(gridPropertyDetails);
        }
    }

    //显示水地面
    public void DisplayWateredGround(GridPropertyDetails gridPropertyDetails)
    {
        //> -1 代表有水
        if (gridPropertyDetails.daysSinceWatered > -1)
        {
            //连接水地面
            ConnectWateredGround(gridPropertyDetails);
        }
    }

    /// <summary>
    /// 设置相邻的 4片瓦片地图块   连接 被挖地面
    /// </summary>
    /// <param name="gridPropertyDetails"></param>
    private void ConnectDugGround(GridPropertyDetails gridPropertyDetails)
    {
        //选择的基础瓦片
        Tile dugTile0 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), dugTile0);

        GridPropertyDetails adjacentGidGridPropertyDetails; //相邻瓦片属性详情

        //-----------------------------Set 4 tiles if dug surrounding current tile up ,down， left ,right   now that this central tile has been dug
        //设置4个瓷砖.如果挖掘当前瓷砖 上，下，左，右。现在这个中心瓷砖已经挖好。


        //up的瓦片
        adjacentGidGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        //相邻存在瓦片并且被挖过
        if (adjacentGidGridPropertyDetails != null && adjacentGidGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile1 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), dugTile1);
        }

        //down的瓦片
        adjacentGidGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        //相邻存在瓦片并且被挖过
        if (adjacentGidGridPropertyDetails != null && adjacentGidGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile2 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), dugTile2);
        }

        //left的瓦片
        adjacentGidGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
        //相邻存在瓦片并且被挖过
        if (adjacentGidGridPropertyDetails != null && adjacentGidGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile3 = SetDugTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), dugTile3);
        }

        //right的瓦片
        adjacentGidGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
        //相邻存在瓦片并且被挖过
        if (adjacentGidGridPropertyDetails != null && adjacentGidGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile4 = SetDugTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), dugTile4);
        }
    }


    /// <summary>
    /// 设置相邻的 4片瓦片地图块  连接水地面
    /// </summary>
    /// <param name="gridPropertyDetails"></param>
    private void ConnectWateredGround(GridPropertyDetails gridPropertyDetails)
    {
        //选择的基础瓦片
        Tile wateredTile0 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), wateredTile0);

        GridPropertyDetails adjacentGidGridPropertyDetails; //相邻瓦片属性详情

        //-----------------------------Set 4 tiles if dug surrounding current tile up ,down， left ,right   now that this central tile has been dug
        //设置4个瓷砖.如果挖掘当前瓷砖 上，下，左，右。现在这个中心瓷砖已经挖好。


        //up的瓦片
        adjacentGidGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        //相邻存在瓦片并且被挖过
        if (adjacentGidGridPropertyDetails != null && adjacentGidGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile1 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), wateredTile1);
        }

        //down的瓦片
        adjacentGidGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        //相邻存在瓦片并且被挖过
        if (adjacentGidGridPropertyDetails != null && adjacentGidGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile2 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), wateredTile2);
        }

        //left的瓦片
        adjacentGidGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
        //相邻存在瓦片并且被挖过
        if (adjacentGidGridPropertyDetails != null && adjacentGidGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile3 = SetWateredTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), wateredTile3);
        }

        //right的瓦片
        adjacentGidGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
        //相邻存在瓦片并且被挖过
        if (adjacentGidGridPropertyDetails != null && adjacentGidGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile4 = SetWateredTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), wateredTile4);
        }
    }

    /// <summary>
    /// 设置被挖的瓦片
    /// </summary>
    private Tile SetDugTile(int xGrid, int yGrid)
    {
        bool upDug = IsGridSquareDug(xGrid, yGrid + 1);
        bool downDug = IsGridSquareDug(xGrid, yGrid - 1);
        bool leftDug = IsGridSquareDug(xGrid - 1, yGrid);
        bool rightDug = IsGridSquareDug(xGrid + 1, yGrid);


        #region set appropriate tile based once whether surrounding tiles are dug or not

        if (!upDug && !downDug && !rightDug && !leftDug)
        {
            return dugGround[0];
        }
        else if (!upDug && downDug && rightDug && !leftDug)
        {
            return dugGround[1];
        }
        else if (!upDug && downDug && rightDug && leftDug)
        {
            return dugGround[2];
        }
        else if (!upDug && downDug && !rightDug && leftDug)
        {
            return dugGround[3];
        }
        else if (!upDug && downDug && !rightDug && !leftDug)
        {
            return dugGround[4];
        }
        else if (upDug && downDug && rightDug && !leftDug)
        {
            return dugGround[5];
        }
        else if (upDug && downDug && rightDug && leftDug)
        {
            return dugGround[6];
        }
        else if (upDug && downDug && !rightDug && leftDug)
        {
            return dugGround[7];
        }
        else if (upDug && downDug && !rightDug && !leftDug)
        {
            return dugGround[8];
        }
        else if (upDug && !downDug && rightDug && !leftDug)
        {
            return dugGround[9];
        }
        else if (upDug && !downDug && rightDug && leftDug)
        {
            return dugGround[10];
        }
        else if (upDug && !downDug && !rightDug && leftDug)
        {
            return dugGround[11];
        }
        else if (upDug && !downDug && !rightDug && !leftDug)
        {
            return dugGround[12];
        }
        else if (!upDug && !downDug && rightDug && !leftDug)
        {
            return dugGround[13];
        }
        else if (!upDug && !downDug && rightDug && leftDug)
        {
            return dugGround[14];
        }
        else if (!upDug && !downDug && !rightDug && leftDug)
        {
            return dugGround[15];
        }

        return null;

        #endregion Set appropriate tile based on whether surrounding tiles are dug or not 根据周围的瓦片是否被挖出来设置适当的切片
    }


    ///判断地块是否被挖了
    private bool IsGridSquareDug(int xGrid, int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);

        if (gridPropertyDetails == null)
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


    private Tile SetWateredTile(int xGrid, int yGrid)
    {
        bool upWatered = IsGridSquareWatered(xGrid, yGrid + 1);
        bool downWatered = IsGridSquareWatered(xGrid, yGrid - 1);
        bool leftWatered = IsGridSquareWatered(xGrid - 1, yGrid);
        bool rightWatered = IsGridSquareWatered(xGrid + 1, yGrid);


        #region set appropriate tile based once whether surrounding tiles are dug or not

        if (!upWatered && !downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[0];
        }
        else if (!upWatered && downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[1];
        }
        else if (!upWatered && downWatered && rightWatered && leftWatered)
        {
            return wateredGround[2];
        }
        else if (!upWatered && downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[3];
        }
        else if (!upWatered && downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[4];
        }
        else if (upWatered && downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[5];
        }
        else if (upWatered && downWatered && rightWatered && leftWatered)
        {
            return wateredGround[6];
        }
        else if (upWatered && downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[7];
        }
        else if (upWatered && downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[8];
        }
        else if (upWatered && !downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[9];
        }
        else if (upWatered && !downWatered && rightWatered && leftWatered)
        {
            return wateredGround[10];
        }
        else if (upWatered && !downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[11];
        }
        else if (upWatered && !downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[12];
        }
        else if (!upWatered && !downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[13];
        }
        else if (!upWatered && !downWatered && rightWatered && leftWatered)
        {
            return wateredGround[14];
        }
        else if (!upWatered && !downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[15];
        }

        return null;

        #endregion Set appropriate tile based on whether surrounding tiles are dug or not 根据周围的瓦片是否被挖出来设置适当的切片
    }

    private bool IsGridSquareWatered(int xGrid, int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);

        if (gridPropertyDetails == null)
        {
            return false;
        }
        else if (gridPropertyDetails.daysSinceWatered > -1)
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
            GridPropertyDetails gridPropertyDetails = item.Value;

            DisplayDugGround(gridPropertyDetails);

            DisplayWateredGround(gridPropertyDetails);

            DisplayPlantedCrop(gridPropertyDetails);
        }
    }

    public void DisplayPlantedCrop(GridPropertyDetails gridPropertyDetails)
    {
        if (gridPropertyDetails.seedItemCode > -1) //未种植
        {
            //获得作物的细节
            CropDetails cropDetails = so_CropDetailsList.GetCropDetails(gridPropertyDetails.seedItemCode);

            if (cropDetails != null)
            {
                //使用预制体
                GameObject cropPrefab;

                //在网格位置 实例化农作物预制体
                int growthStages = cropDetails.growthDays.Length;

                int currentGrowthStage = 0; //当前生长步骤
                int daysCounter = cropDetails.totalGrowthDays; //总生长时间

                //找出目前生长阶段
                for (int i = growthStages - 1; i >= 0; i--)
                {
                    if (gridPropertyDetails.growthDays >= daysCounter)
                    {
                        currentGrowthStage = i;
                        break;
                    }

                    daysCounter = daysCounter - cropDetails.growthDays[i];
                }

                cropPrefab = cropDetails.growthPrefab[currentGrowthStage]; //农作物阶段预制体

                Sprite growthSprite = cropDetails.GrowthSprites[currentGrowthStage]; //农作物阶段精灵图

                Vector3 worldPosition =
                    groundDecoration2.CellToWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0)); //网格的世界坐标

                worldPosition = new Vector3(worldPosition.x + Settings.gridCellSize / 2, worldPosition.y, worldPosition.z); //网格的世界坐标修正

                GameObject cropInstance = Instantiate(cropPrefab, worldPosition, Quaternion.identity); //实例化农作物

                //填充实例化的精灵 父级 网格位置
                cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = growthSprite;
                cropInstance.transform.SetParent(cropsParentTransform);
                cropInstance.GetComponent<Crop>().cropGridPosition = new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
            }
        }
    }


    /// <summary>
    /// This initialises the grid property dictionary with the valuesfrom the 50 GridProperties assets and stores the valuesor eachscene in GameObjectsave sceneData
    /// 这将使用来自50个 GridProperties 资产的值初始化网格属性字典，并将值或每个场景存储在 GameObjectsave SceneData 中
    /// </summary>
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
            if (so_GridProperties.sceneName.ToString() == SceneControllerManager.Instance.startingSceneName.ToString())
            {
                this.gridPropertyDictionary = gridPropertyDictionary;
            }

            //将场景数据 加入对象数据的场景数据字典里
            GameObjectSave.sceneDate.Add(so_GridProperties.sceneName.ToString(), sceneSave);
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


    /// <summary>
    /// 加载场景后 需要获得一些对象
    /// </summary>
    private void AfterSceneLoaded()
    {
        //加载后 获得网格
        grid = GameObject.FindObjectOfType<Grid>();

        groundDecoration1 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration1).GetComponent<Tilemap>();
        groundDecoration2 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration2).GetComponent<Tilemap>();


        //防风草的父级
        if (GameObject.FindGameObjectWithTag(Tags.CropsParentTransform) != null) //有的场景是室内 没有父级对象
        {
            cropsParentTransform = GameObject.FindGameObjectWithTag(Tags.CropsParentTransform).transform;
        }
        else
        {
            cropsParentTransform = null;
        }
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
    /// Returns the Crop object at the gridx,gridY position or null if no crop was found
    /// 返回 gridx 处的 Crop 对象，如果没有找到任何作物，返回 gridY 位置或 null
    /// </summary>
    public Crop GetCropObjectAtGridLocation(GridPropertyDetails gridPropertyDetails)
    {
        //获取网格上的碰撞物数组
        Vector3 worldPosition = grid.GetCellCenterWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));
        Collider2D[] collider2DArray = Physics2D.OverlapPointAll(worldPosition);

        //没有农作物返回null
        Crop crop = null;
        //遍历碰撞物的crop组件 填充
        for (int i = 0; i < collider2DArray.Length; i++)
        {
            crop = collider2DArray[i].gameObject.GetComponentInParent<Crop>();
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
                break;
            crop = collider2DArray[i].gameObject.GetComponentInChildren<Crop>();
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
                break;
        }

        //返回农作物
        return crop;
    }


    /// <summary>
    /// 返回提供的 SeeItemCode 的作物详细信息
    /// </summary>
    public CropDetails GetCropDetails(int seedItemCode)
    {
        return so_CropDetailsList.GetCropDetails(seedItemCode);
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
        GameObjectSave.sceneDate.Add(sceneName, sceneSave);
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

    private void AdvanceDay(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute,
        int gameSecond)
    {
        //清除过时的地面效果 和旧生长阶段的农作物
        ClearDisplayGridPropertyDetails();

        //Loop through all scenes-by looping through all gridproperties in the array
        foreach (SO_GridProperties so_GridProperties in so_gridPropertiesArray)
        {
            //Get gridpropertydetails dictionary for scene
            if (GameObjectSave.sceneDate.TryGetValue(so_GridProperties.sceneName.ToString(), out SceneSave sceneSave))
            {
                if (sceneSave.gridPropertyDetailsDictionary != null)
                {
                    for (int i = sceneSave.gridPropertyDetailsDictionary.Count - 1; i >= 0; i--)
                    {
                        KeyValuePair<string, GridPropertyDetails> item = sceneSave.gridPropertyDetailsDictionary.ElementAt(i);

                        GridPropertyDetails gridPropertyDetails = item.Value;


                        #region Update all grid properties to reflect the advance in the day  重置水地面

                        //每天更新作物的天数 +1
                        if (gridPropertyDetails.growthDays > -1)
                        {
                            gridPropertyDetails.growthDays += 1;
                        }


                        //如果地面上有水 清除它
                        if (gridPropertyDetails.daysSinceWatered > -1)
                        {
                            gridPropertyDetails.daysSinceWatered = -1;
                        }

                        //设置
                        SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails,
                            sceneSave.gridPropertyDetailsDictionary);

                        #endregion Update all grid properties to reflect the advance in the day
                    }
                }
            }
        }

        //显示改变前的
        DisplayGridPropertyDetails();
    }
}