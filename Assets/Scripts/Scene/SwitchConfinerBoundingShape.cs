using UnityEngine;
using Cinemachine;

public class SwitchConfinerBoundingShape : MonoBehaviour
{
    //订阅 场景加载后的通知 然后执行切换边界摄像机的碰撞范围
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SwitchBoundingShape;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SwitchBoundingShape;
    }


    /// <summary>
    /// Switch the collider that cinemachine uses to define the edges of the screen
    /// 切换边界碰撞
    /// </summary>
    private void SwitchBoundingShape()
    {

        // Get the polygon collider on the 'boundsconfiner' gameobject which is used by Cinemachine to prevent the camera going beyond the screen edges
        //能否找到防止摄像机超出屏幕的边界碰撞器
        PolygonCollider2D polygonCollider2D =
            GameObject.FindGameObjectWithTag(Tags.BoundsConfiner).GetComponent<PolygonCollider2D>();

        CinemachineConfiner cinemachineConfiner = GetComponent<CinemachineConfiner>();

        //获得另外一个场景中的碰撞器
        cinemachineConfiner.m_BoundingShape2D = polygonCollider2D;



        //since the confiner bounds have changed need to call this to clear the cache
        //清除碰撞器缓存 为应用新的准备
        cinemachineConfiner.InvalidatePathCache();

    }
}