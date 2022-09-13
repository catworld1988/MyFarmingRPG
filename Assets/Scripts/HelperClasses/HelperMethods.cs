using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods
{
    /// <summary>
    /// Gets Components of type T at box with centre point and size and angle.Returns true if at least one found and the found components are returned in the list
    /// 获取带有中心点、大小和角度的盒子上的 T 型组件。如果在列表中至少返回一个已找到的组件并且已找到的组件已返回，则返回 true
    /// </summary>
    public static bool GetComponentsAtBoxLocation<T>(out List<T> listComponentsAtBoxPosition, Vector2 point, Vector2 size, float angle)
    {
        bool found = false;
        List<T> componentList = new List<T>();

        Collider2D[] collider2DArray = Physics2D.OverlapBoxAll(point, size, angle);

        //遍历所有的碰撞 找出T类型对象
        for (int i = 0; i < collider2DArray.Length; i++)
        {
            T tComponent = collider2DArray[i].gameObject.GetComponentInParent<T>();
            if (tComponent !=null)
            {
                found = true;
                componentList.Add(tComponent);
            }
            else
            {
                tComponent = collider2DArray[i].gameObject.GetComponentInChildren<T>();
                if (tComponent !=null)
                {
                    found = true;
                    componentList.Add(tComponent);
                }
            }
        }

        listComponentsAtBoxPosition = componentList;

        return found;

    }
}

