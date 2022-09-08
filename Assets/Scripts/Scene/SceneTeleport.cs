using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BoxCollider2D))]
public class SceneTeleport : MonoBehaviour
{

    [SerializeField] private SceneName sceneNameGoto = SceneName.Scene1_Farm;
    [SerializeField] private Vector3 scenePositionGoto = new Vector3();


    private void OnTriggerStay2D(Collider2D collision)  //不用离开检测区 移动速度触发 可以快速检测切换场景
    {
        Player player = collision.GetComponent<Player>();

        if (player!= null)
        {
            //如果坐标是0，就计算玩家新的坐标位置 如果有设定的数值了，就用新的数值不获取
            float xPosition = Mathf.Approximately(scenePositionGoto.x, 0f) ? player.transform.position.x : scenePositionGoto.x;

            float yPosition = Mathf.Approximately(scenePositionGoto.y, 0f) ? player.transform.position.y : scenePositionGoto.y;

            float zPosition = 0f;


            //根据场景名 和玩家坐标 传送到新场景
            SceneControllerManager.Instance.FadeAndLoadScene(sceneNameGoto.ToString(),new Vector3(xPosition,yPosition,zPosition));
        }
    }
}
