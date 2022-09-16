using System.Collections;
using System.Collections.Generic;
using UnityEngine;




/// <summary>
/// 对象池
/// </summary>
public class PoolManager : SingletonMonobehaviour<PoolManager>
{
    //声明一个对象池
    private Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();

    //放入池中的 对象数组
    [SerializeField] private Pool[] pool = null;
    [SerializeField] private Transform objectPoolTransform = null;    //池中对象的变换属性


    [System.Serializable]
    //池对象的结构体属性
    public struct Pool
    {
        public int poolSize; //创建的数量
        public GameObject prefab;
    }

    private void Start()
    {
        for (int i = 0; i < pool.Length; i++)
        {
            CreatePool(pool[i].prefab, pool[i].poolSize);
        }
    }

    /// <summary>
    /// 创建对象池
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="poolSize"></param>
    private void CreatePool(GameObject prefab, int poolSize)
    {
        int poolKey = prefab.GetInstanceID();   //预制件实例ID 作为字典的键值
        string prefabName = prefab.name;  //获取预支体的名字

        GameObject parentGameObject = new GameObject(prefabName + "Anchor");   //创建父级对象管理子物体

        parentGameObject.transform.SetParent(objectPoolTransform);


        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey,new Queue<GameObject>());

            for (int i = 0; i < poolSize; i++)
            {
                GameObject newObject = Instantiate(prefab,parentGameObject.transform) as GameObject;

                newObject.SetActive(false);

                poolDictionary[poolKey].Enqueue(newObject); //在队列最后一位 入队
            }
        }
    }

    /// <summary>
    /// 恢复 对象池中对象
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public GameObject ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int poolKey = prefab.GetInstanceID();  //获取预制件的实例ID

        if (poolDictionary.ContainsKey(poolKey))
        {
            //从池子队列中 获得对象
            GameObject objectToReuse = GetObjectFromPool(poolKey);

            //重置 取出的对象的状态
            ResetObject(position, rotation, objectToReuse, prefab);

            //返回
            return objectToReuse;
        }
        else
        {
            //池子里面没找到 对象 发消息给控制台
            Debug.Log("No object pool for " + prefab);
            return null;
        }
    }


    private GameObject GetObjectFromPool(int poolKey)
    {
        //从队列的开头 移除并返回对象
        GameObject objectToReuse = poolDictionary[poolKey].Dequeue();

        //将对象添加到Queue<T>的末尾。
        poolDictionary[poolKey].Enqueue(objectToReuse);

        if (objectToReuse.activeSelf== true)
        {
            objectToReuse.SetActive(false);
        }

        return objectToReuse;
    }


    /// <summary>
    /// 重置对象
    /// </summary>
    private void ResetObject(Vector3 position, Quaternion rotation, GameObject objectToReuse, GameObject prefab)
    {
        objectToReuse.transform.position = position;
        objectToReuse.transform.rotation = rotation;

        //重置预制件的最初比例
        objectToReuse.transform.localScale = prefab.transform.localScale;


    }

}
