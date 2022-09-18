using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 作物的核心类
/// </summary>
public class Crop : MonoBehaviour
{
    private int harvestActionCount = 0;


    [HideInInspector] public Vector2Int cropGridPosition;

    public void ProcessToolAction(ItemDetails equippedItemItemDetails)
    {
        //检测 获得网格
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y);
        if (gridPropertyDetails == null)
            return;

        //检测 获得种子
        ItemDetails seedItemDetails = InventoryManager.Instance.GetItemDetails(gridPropertyDetails.seedItemCode);
        if (seedItemDetails == null)
            return;

        //检测 获得农作物
        CropDetails cropDetails = GridPropertiesManager.Instance.GetCropDetails(seedItemDetails.itemCode);
        if (cropDetails == null)
            return;

        //获得网格
        harvestActionCount += 1;

        //如果没有收获的动作  这个工具不能收获该农作物
        int requiredHarvestActions = cropDetails.RequiredHarvestActionsForTool(equippedItemItemDetails.itemCode);
        if (requiredHarvestActions == -1)
            return;

        //检查收获动作是否大于 必须的收获动作
        if (harvestActionCount >= requiredHarvestActions)
            HarvestCrop(cropDetails, gridPropertyDetails);

    }

    private void HarvestCrop(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        gridPropertyDetails.seedItemCode = -1;
        gridPropertyDetails.growthDays = -1;
        gridPropertyDetails.daysSinceLastHarvest = -1;
        gridPropertyDetails.daysSinceWatered = -1;

        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX,gridPropertyDetails.gridY,gridPropertyDetails);

        HarvestActions(cropDetails, gridPropertyDetails);


    }

    private void HarvestActions(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        //收获农作物的产物
        SpawnHarvestedItems(cropDetails);

        Destroy(gameObject);

    }

    private void SpawnHarvestedItems(CropDetails cropDetails)
    {
        for (int i = 0; i < cropDetails.cropProducedItemCode.Length; i++)
        {
            int cropsToProduce;

            //农作物掉落物 的随机数量
            if (cropDetails.cropProducedMinQuantity[i]==cropDetails.cropProducedMaxQuantity[i] ||
                cropDetails.cropProducedMaxQuantity[i]<cropDetails.cropProducedMinQuantity[i])
            {
                cropsToProduce = cropDetails.cropProducedMinQuantity[i];
            }
            else
            {
                cropsToProduce = Random.Range(cropDetails.cropProducedMinQuantity[i], cropDetails.cropProducedMaxQuantity[i] + 1);
            }

            //农作物掉落物 随机产生位置
            for (int j = 0; j < cropsToProduce; j++)
            {
                Vector3 spawnPosition;
                if (cropDetails.spawnCropProducedAtPlayerPosition)
                {
                    //添加到玩家库存中
                    InventoryManager.Instance.AddItem(InventoryLocation.player,cropDetails.cropProducedItemCode[i]);
                }
                else
                {
                    //随机位置
                    spawnPosition = new Vector3(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f), 0f);
                    SceneItemsManager.Instance.InstantiateSceneItem(cropDetails.cropProducedItemCode[i],spawnPosition);
                }
            }
        }
    }
}