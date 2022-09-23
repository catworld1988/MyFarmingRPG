using System.Collections.Generic;


[System.Serializable]

public class SceneSave
{
    //string key is an identifier name we choose for this list
    //string key 是我们为这个列表选择的标识符名称
    //第一次加载场景判定
    public Dictionary<string, bool> boolDictionary;
    //存方向的字典
    public Dictionary<string, string> stringDictionary;
    //存位置的字典
    public Dictionary<string,Vector3Serializable> vector3Dictionary;
    //保存物品的标识符索引和物品列表的字典
    public Dictionary<string, int[]> intArrayDictionary;
    public List<SceneItem> listSceneItem;
    public Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;
    public List<InventoryItem>[] listInvItemArray;

}
