[System.Serializable] //序列化便与储存 将对象的状态信息转换为可以存储或传输,将其当前状态写入到临时或持久性存储区。

public class SceneItem
{
    public int itemCode;
    public Vector3Serializable position;
    public string itemName;

    public SceneItem() //构造函数 创建的时候用序列化坐标 定义物品位置
    {
        position = new Vector3Serializable();
    }

}
