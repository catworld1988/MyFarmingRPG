using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 获得鼠标位置 碰撞的对象的T组件
/// </summary>
public static class HelperMethods
{
    public static bool GetComponentsAtCursorLocation<T>(out List<T> ComponentsAtPositionList, Vector3 positionToCheck)
    {
        bool found = false;

        List<T> componentList = new List<T>();

        Collider2D[] collider2DArray = Physics2D.OverlapPointAll(positionToCheck); //重叠点区域 碰撞数组

        //循环找到所有的碰撞 看是是否找到T类型的组件
        T tComponent= default(T);

        for (int i = 0; i < collider2DArray.Length; i++)
        {
            tComponent = collider2DArray[i].gameObject.GetComponentInParent<T>();

            if (tComponent!= null)
            {
                found = true;
                componentList.Add(tComponent);
            }
            else
            {
                tComponent = collider2DArray[i].gameObject.GetComponentInChildren<T>();
                if (tComponent!= null)
                {
                    found = true;
                    componentList.Add(tComponent);
                }
            }
        }

        ComponentsAtPositionList = componentList;

        return found;
    }

    /// <summary>
    /// Returns array of components of type T at box with centre point and size and angle.The numberofCollidersToTest for is passed as a parameter.Found components are returned in the array.
    /// 在坐标 大小 角度位置 返回T型组件组成的数组
    /// </summary>
    public static T[] GetcomponentstBoxPositionNonAlloc<T>(int numberOfCollidersToTest, Vector2 point, Vector2 size, float angle)
    {
        Collider2D[] collider2DArray = new Collider2D[numberOfCollidersToTest];

        Physics2D.OverlapBoxNonAlloc(point, size, angle, collider2DArray);

        T tComponent = default( T );

        T[] componentArray = new T[collider2DArray.Length];

        for (int i = collider2DArray.Length - 1; i >= 0; i--)
        {
            if (collider2DArray[i] !=null)
            {
                tComponent = collider2DArray[i].gameObject.GetComponent<T>();

                if (tComponent != null)
                {
                    componentArray[i] = tComponent;
                }
            }
        }

        return componentArray;
    }



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

