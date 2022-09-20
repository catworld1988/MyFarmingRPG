using System.Collections.Generic;

[System.Serializable]

public class SceneSave
{
    //string key is an identifier name we choose for this list
    //string key 是我们为这个列表选择的标识符名称
    public Dictionary<string, bool> boolDictionary;
    //保存物品的标识符索引和物品列表的字典
    public List<SceneItem> listSceneItem;
    public Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;

}
