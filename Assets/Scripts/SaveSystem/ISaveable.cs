public interface ISaveable //保存数据必须实现的接口
{
    string ISaveableUniqueID { get; set; } //需要获得 ID属性
    GameObjectSave GameObjectSave { get; set; } //获取游戏对象字典



    void ISaveableRegister(); //注册 需要实现的方法
    void ISaveableDeregister(); //注销 需要实现的方法


    GameObjectSave ISaveableSave();

    void ISaveableLoad(GameSave gameSave);


    void ISaveableStoreScene(string sceneName);  //存储场景数据 需要实现的方法
    void ISaveableRestoreScene(string sceneName); //恢复还原 场景数据 需要实现的方法
}
