[System.Serializable]


public class GridProperty  //网格属性类
{
    //坐标
    public GridCoordinate gridCoordinate;
    //网格布尔属性
    public GridBoolProperty gridBoolProperty;
    //网格布尔值
    public bool gridBoolValue = false;

    public GridProperty(GridCoordinate gridCoordinate, GridBoolProperty gridBoolProperty, bool gridBoolValue)
    {
        this.gridCoordinate = gridCoordinate;
        this.gridBoolProperty = gridBoolProperty;
        this.gridBoolValue = gridBoolValue;
    }
}
