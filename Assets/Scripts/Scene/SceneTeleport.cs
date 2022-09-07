using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BoxCollider2D))]
public class SceneTeleport : MonoBehaviour
{

    [SerializeField] private SceneName sceneNameGoto = SceneName.Scene1_Farm;
    [SerializeField] private Vector3 scenePositionGoto = new Vector3();


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player!= null)
        {
            //计算玩家 新的x位置 如果为真就返回分号前面的
            float xPosition = Mathf.Approximately(scenePositionGoto.x, 0f) ? player.transform.position.x : scenePositionGoto.x;

            float yPosition = Mathf.Approximately(scenePositionGoto.y, 0f) ? player.transform.position.y : scenePositionGoto.y;

            float zPosition = 0f;


            //传送到新场景
            SceneControllerManager.Instance.FadeAndLoadScene(sceneNameGoto.ToString(),new Vector3(xPosition,yPosition,zPosition));
        }
    }
}
