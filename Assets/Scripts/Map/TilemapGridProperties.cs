using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways] //编辑器下运行
public class TilemapGridProperties : MonoBehaviour
{
    private Tilemap tilemap;
    [SerializeField] private SO_GridProperties gridProperties = null;
    [SerializeField] private GridBoolProperty gridBoolProperty=GridBoolProperty.diggable;

    private void OnEnable()
    {
        //Only populate in the editor
        //仅在编辑器中填充
        if (!Application.IsPlaying(gameObject))
        {
            tilemap = GetComponent<Tilemap>();

            //清除旧 网格属性数据
            if (gridProperties !=null)
            {
                gridProperties.GridPropertyList.Clear();
            }
        }
    }

    private void OnDisable()
    {
        //当前对象是否在游戏应用中运行
        if (!Application.IsPlaying(gameObject))
        {
            UpdateGridProperties();

            if (gridProperties!=null)
            {
                //This is required to ensure that the updated gridproperties gameobject gets saved when the game is saved otherwise they are not saved.
                //这是为了确保更新的网格属性游戏对象在游戏被保存时被保存，否则它们不会被保存。
                EditorUtility.SetDirty(gridProperties);
            }
        }
    }

    private void UpdateGridProperties()
    {
        //将 Tilemap 的 origin 和 size 压缩到瓦片所存在的边界。
        tilemap.CompressBounds();

        //只在编辑器中游戏模式运行
        if (!Application.IsPlaying(gameObject))
        {
            if (gridProperties != null)
            {
                //设置单元格边界
                Vector3Int startCell = tilemap.cellBounds.min;
                Vector3Int endCell = tilemap.cellBounds.max;

                for (int x = startCell.x; x < endCell.x; x++)
                {
                    for (int y = startCell.y; y < endCell.y; y++)
                    {
                        //从Tilemap 此基类继承可实现要放置在 Tilemap 组件中的自定义瓦片。
                        //GetTile根据给定的瓦片地图中某个单元格的 XYZ 坐标，获取瓦片。
                        TileBase tile = tilemap.GetTile(new Vector3Int(x, y, 0));

                        if (tile !=null)
                        {
                            gridProperties.GridPropertyList.Add(new GridProperty(new GridCoordinate(x,y),gridBoolProperty,true));
                        }
                    }
                }
            }
        }
    }

    private void Update()
    {
        //只在编辑器中运行
        if (!Application.IsPlaying(gameObject))
        {
            Debug.Log("DiSABLE PROPERTY TILEMAPS");
        }
    }
}
