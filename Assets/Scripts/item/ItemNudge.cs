using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemNudge : MonoBehaviour
{

    private WaitForSeconds pause;
    private bool isAnimating = false;

    private void Awake()
    {
        pause = new WaitForSeconds(0.04f);
    }
/// <summary>
/// player碰到物品 物品触发旋转晃动动画效果  玩家在右边触发顺时针  在左边触发逆时针。
/// </summary>
/// <param name="col"></param>
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (isAnimating == false)
        {
            if (gameObject.transform.position.x < col.transform.position.x)
            {
                StartCoroutine(RotateAntiClock());
            }
            else //player.x > 物体.x      player在右边 物体顺时针旋转
            {
                StartCoroutine(RotateClock());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (isAnimating == false)
        {
            if (gameObject.transform.position.x > col.transform.position.x) //player.x > 物体.x      player在右边 物体顺时针旋转
            {
                StartCoroutine(RotateAntiClock());
            }
            else
            {
                StartCoroutine(RotateClock());
            }
        }
    }


    private IEnumerator RotateAntiClock() //逆时针物体晃动动画   可以尝试用 To between插件;
    {
        isAnimating = true;

        //物体沿Z旋转
        for (int i = 0; i < 4; i++)
        {
            gameObject.transform.GetChild(0).Rotate(0f,0f,2f);
            yield return pause;
        }
        for (int i = 0; i < 5; i++)
        {
            gameObject.transform.GetChild(0).Rotate(0f,0f,-2f);
            yield return pause;
        }

        gameObject.transform.GetChild(0).Rotate(0f,0f,2f);

        yield return pause;
        isAnimating = false;
    }
    private IEnumerator RotateClock() //顺时针旋转晃动
    {
        isAnimating = true;

        //物体沿Z旋转
        for (int i = 0; i < 4; i++)
        {
            gameObject.transform.GetChild(0).Rotate(0f,0f,-2f);
            yield return pause;
        }
        for (int i = 0; i < 5; i++)
        {
            gameObject.transform.GetChild(0).Rotate(0f,0f,2f);
            yield return pause;
        }

        gameObject.transform.GetChild(0).Rotate(0f,0f,-2f);

        yield return pause;
        isAnimating = false;
    }

}