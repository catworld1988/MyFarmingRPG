using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//自定义编辑器扩展 绘制描述属性
[CustomPropertyDrawer(typeof(ItemCodeDescriptionAttribute))]
public class ItemCodeDescriptionDrawer : PropertyDrawer
{
    //描述属性的框体重写
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //Change the returned property height to be double to cater for the additional item code description that we will draw
        //将描述的框体改为原来高度的两倍
        return EditorGUI.GetPropertyHeight(property) * 2;
    }

    //绘制
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Using BeginProperty / EndProperty on the parent property means that prefab override logic works on the entire property
        //用此方法确保预制件重载逻辑能正常工作
        EditorGUI.BeginProperty(position, label, property);

        //属性类型是否是序列化的整数
        if (property.propertyType == SerializedPropertyType.Integer)
        {
            EditorGUI.BeginChangeCheck(); //开始检查数值变化

            //绘制 物品 代码 Draw item Code     IntField	创建一个用于输入整数的文本字段。
            var newValue = EditorGUI.IntField(new Rect(position.x, position.y, position.width, position.height / 2),
                label, property.intValue);

            //Draw item description 绘制物品属性
            EditorGUI.LabelField(
                new Rect(position.x, position.y + position.height / 2, position.width, position.height / 2),
                "Item Description", GetItemDescription(property.intValue));


            //If item code value has changed,then set value to new value 如果数值变化，将数值变更为新的数值
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = newValue;
            }
        }


        EditorGUI.EndProperty();
    }

    /// <summary>
    /// 获得物品属性描述
    /// </summary>
    /// <param name="itemCode"></param>
    /// <returns></returns>
    private string GetItemDescription(int itemCode)
    {
        SO_ItemList so_itemList;

        //按路径加载 SO_ItemList
        so_itemList =
            AssetDatabase.LoadAssetAtPath("Assets/Scriptable Object Assets/Item/so_ItemList.asset", typeof(SO_ItemList))
                as SO_ItemList;

        //创建物品细节列表
        List<ItemDetails> itemDetailsList = so_itemList.itemDetails;

        //查找物品ID
        ItemDetails itemDetail = itemDetailsList.Find(x => x.itemCode == itemCode);

        //如果找到了 返回详细细节属性 返回的是String
        if (itemDetail != null)
        {
            return itemDetail.itemDescription;
        }
        else
        {
            return "";
        }
    }
}