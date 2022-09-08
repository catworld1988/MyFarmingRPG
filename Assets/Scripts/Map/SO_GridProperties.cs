using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "so_GridProperties", menuName = "Scriptable Objects/Grid Properties")]
public class SO_GridProperties : ScriptableObject
{
    public SceneName sceneName;
    public int gridWidth;
    public int gridHeight;
    public int originX;   //起点（左下角）坐标x
    public int originY;  //起点（左下角）坐标y

    [SerializeField] public List<GridProperty> GridPropertyList;
}