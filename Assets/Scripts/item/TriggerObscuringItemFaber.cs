using UnityEngine;


/// <summary>
/// 挂该脚本 碰撞物体  触发身上的渐隐脚本
/// </summary>
public class TriggerObscuringItemFaber : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        //Get the gameobject we have collided with,and then get all the Obscuring Item Fader components on it and its children and then trigger the fade out
        //当玩家碰撞到物体，获取物体和它的子物体上有 Obscuring Item Faber 组件 - 触发组件的调节 淡出 变为半透明。
        ObscuringItemFader[] obscuringItemFader = collision.gameObject.GetComponentsInChildren<ObscuringItemFader>();

        //遍历物件数组 执行淡出
        if (obscuringItemFader.Length>0)
        {
            for (int i = 0; i < obscuringItemFader.Length; i++)
            {
                obscuringItemFader[i].FadeIn();
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        //当玩家不再碰撞到物体，获取物体和它的子物体上有 Obscuring Item Faber 组件 - 触发组件的 淡入 变为不透明。
        ObscuringItemFader[] obscuringItemFader = collision.gameObject.GetComponentsInChildren<ObscuringItemFader>();

        //遍历物件数组 执行淡入
        if (obscuringItemFader.Length>0)
        {
            for (int i = 0; i < obscuringItemFader.Length; i++)
            {
                obscuringItemFader[i].FadeOut();
            }
        }
    }
}