using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Attach to a crop prefab to set the values in the grid property dictionary
/// 把作物预设设置值附加到在网格属性字典
/// </summary>
public class CropInstantiator : MonoBehaviour
{

    private Grid grid;
    [SerializeField] private int daySinceDug = -1;
    [SerializeField] private int daySinceWatered = -1;
    [ItemCodeDescription]
    [SerializeField] private int seedItemCode = 0;
    [SerializeField] private int growthDays = 0;


    private void OnEnable()
    {
        EventHandler.InstantiateCropPrefabsEvent += InstantiateCropPrefabs;
    }



    private void OnDisable()
    {
        EventHandler.InstantiateCropPrefabsEvent -= InstantiateCropPrefabs;

    }

    private void InstantiateCropPrefabs()
    {
        grid = GameObject.FindObjectOfType<Grid>();

        //整形的3维向量
        Vector3Int cropGridPosition = grid.WorldToCell(transform.position);

        SetCropGridProperties(cropGridPosition);

        Destroy(gameObject);

    }

    private void SetCropGridProperties(Vector3Int cropGridPosition)
    {
        if (seedItemCode>0)
        {
            GridPropertyDetails gridPropertyDetails;

            gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y);

            if (gridPropertyDetails == null)
            {
                gridPropertyDetails = new GridPropertyDetails();
            }

            gridPropertyDetails.daysSinceDug = daySinceDug;
            gridPropertyDetails.daysSinceWatered = daySinceWatered;
            gridPropertyDetails.seedItemCode = seedItemCode;
            gridPropertyDetails.growthDays = growthDays;

            GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX,gridPropertyDetails.gridY,gridPropertyDetails);


        }


    }
}
