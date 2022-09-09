using System.Collections.Generic;

[System.Serializable]

public class GameObjectSave
{
    //定义一个包含 物品标识符字典 的场景名称标识符字典
    //string 标识 是场景名称

    //场景名key   场景保存字典>场景物品字典>场景物品坐标
    public Dictionary<string, SceneSave> sceneDate;

    public GameObjectSave()
    {
        sceneDate = new Dictionary<string, SceneSave>(); //string 标识 是场景名称
    }

    //对象保存字典  对象保存字典> 场景保存字典> 场景物品字典> 场景物品坐标
    public GameObjectSave(Dictionary<string,SceneSave> sceneDate) //string 标识 是场景名称
    {
        this.sceneDate = sceneDate;
    }
}
