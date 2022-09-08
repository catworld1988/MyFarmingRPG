using UnityEngine;
// ReSharper disable InconsistentNaming

[ExecuteAlways] //编辑器中 编辑下运行 在游戏运行前已完成
public class GenerateGUID : MonoBehaviour
{

    [SerializeField] private string _gUID = "";

    public string GUID { get => _gUID; set => _gUID = value; } //设置GUID 属性访问器 可扩展数据验证 或转换等条件


    private void Awake()
    {
        //仅在编辑器中 运行模式 填充
        if (!Application.IsPlaying(gameObject))
        {
            //获取标识符
            if (_gUID=="")
            {
                //全球唯一 128位的数字标识符ID
                _gUID = System.Guid.NewGuid().ToString();
            }
        }
    }
}