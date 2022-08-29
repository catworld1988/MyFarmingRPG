using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "so_ItemList",menuName ="Scriptable Objects/Item/Item List")]
public class SO_ItemList : ScriptableObject  //脚本化数据储存
{
    [SerializeField]
    public List<ItemDetails> itemDetails;
}
