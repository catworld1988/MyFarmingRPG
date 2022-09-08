[System.Serializable] //序列化

public class Vector3Serializable
{
    //unity中的坐标不能序列化 自创建可序列化坐标 用于存储
    public float x, y, z;

    public Vector3Serializable(float x, float y, float z)  //结构函数 赋值可序列化坐标
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3Serializable() //空的构造函数
    {
    }
}
