using System.Collections.Generic;

[System.Serializable]

public class SceneSave
{
    //保存物品的标识符索引和物品列表的字典
    public Dictionary<string, List<SceneItem>> listSceneItemDictionary;
}