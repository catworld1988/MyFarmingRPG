using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : SingletonMonobehaviour<SaveLoadManager>
{


    public GameSave gameSave;
    public List<ISaveable> iSaveableObjectList; //接口类型的保存对象列表
    protected override void Awake()
    {
        base.Awake();

        //初始化接口列表
        iSaveableObjectList = new List<ISaveable>();
    }

    /// <summary>
    /// 从文件读取  游戏的存储数据
    /// </summary>
    public void LoadDataFromFile()
    {
        //创建新的二进制序列化数据
        BinaryFormatter bf = new BinaryFormatter();

        //路径下存在对应文件
        if (File.Exists((Application.persistentDataPath+"/WildHopeCreek.dat")))
        {
            gameSave = new GameSave();
            //打开文件数据流
            FileStream file = File.Open(Application.persistentDataPath + "/WildHopeCreek.dat", FileMode.Open);

            //反序列化
            gameSave = (GameSave)bf.Deserialize(file);

            //循环所有ISaveable 对象并应用保存数据
            for (int i = iSaveableObjectList.Count-1; i >-1; i--)
            {
                if (gameSave.gameObjectData.ContainsKey(iSaveableObjectList[i].ISaveableUniqueID))
                {
                    iSaveableObjectList[i].ISaveableLoad(gameSave);
                }
                //找不到匹配的数据 则销毁
                else
                {
                    Component component = (Component)iSaveableObjectList[i];
                    Destroy(component.gameObject);
                }
            }
            file.Close();
        }

        UIManager.Instance.DisablePauseMenu();
    }

    /// <summary>
    /// 保存数据到 文件
    /// </summary>
    public void SaveDataToFile()
    {
        gameSave = new GameSave();

        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            gameSave.gameObjectData.Add(iSaveableObject.ISaveableUniqueID,iSaveableObject.ISaveableSave());
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/WildHopeCreek.dat", FileMode.Create);

        bf.Serialize(file,gameSave);
        file.Close();

        UIManager.Instance.DisablePauseMenu();
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
