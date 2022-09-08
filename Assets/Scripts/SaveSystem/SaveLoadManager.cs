using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveLoadManager : SingletonMonobehaviour<SaveLoadManager>
{
    public List<ISaveable> iSaveableObjectList; //接口类型的保存对象列表
    protected override void Awake()
    {
        base.Awake();

        //初始化接口列表
        iSaveableObjectList = new List<ISaveable>();
    }

    public void StoreCurrentSceneDate()
    {
        //在所有实现接口列表中循环 执行当前场景 的场景数据实现方法
        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            iSaveableObject.ISaveableStoreScene(SceneManager.GetActiveScene().name);
        }
    }
    public void RestoreCurrentSceneDate()
    {
        //在所有实现接口列表中循环 执行恢复还原当前场景 的场景数据实现方法
        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            iSaveableObject.ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }
}
