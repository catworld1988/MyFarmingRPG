using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 作物的核心类
/// </summary>
public class Crop : MonoBehaviour
{
    private int harvestActionCount = 0;

    [Tooltip("This should be populated from child gameobject")]
    [SerializeField] private SpriteRenderer cropHarvesteSpriteRenderer = null;

    [HideInInspector] public Vector2Int cropGridPosition;

    public void ProcessToolAction(ItemDetails equippedItemDetails,bool isToolRight,bool isToolLeft,bool isToolDown,bool isToolUp)
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

        //获取农作物的动画组件
        Animator animator = GetComponentInChildren<Animator>();

        //触发工具动画
        if (animator!=null)
        {
            if (isToolRight || isToolUp)
            {
                animator.SetTrigger("usetoolright");
            }
            else if (isToolLeft || isToolDown)
            {
                animator.SetTrigger("usetoolleft");
            }
        }

        //如果没有收获的动作  这个工具不能收获该农作物
        int requiredHarvestActions = cropDetails.RequiredHarvestActionsForTool(equippedItemDetails.itemCode);
        if (requiredHarvestActions == -1)
            return;

        //获得网格
        harvestActionCount += 1;


        //检查收获动作是否大于 必须的收获动作
        if (harvestActionCount >= requiredHarvestActions)
            HarvestCrop(isToolRight,isToolUp,cropDetails, gridPropertyDetails,animator);

    }

    private void HarvestCrop(bool isUsingToolRight,bool isUsingToolUp, CropDetails cropDetails, GridPropertyDetails gridPropertyDetails,Animator animator)
    {
        //检测有收获动画器
        if (cropDetails.isHarvestedAniamtion && animator !=null)
        {
            //检测有sprite render //TODO null
            if (cropDetails.harvestedSprite!=null)
            {
                if (cropHarvesteSpriteRenderer!=null)
                {
                    cropHarvesteSpriteRenderer.sprite = cropDetails.harvestedSprite;
                }
            }

            if (isUsingToolRight|| isUsingToolUp)
            {
                animator.SetTrigger("harvestright");
            }
            else
            {
                animator.SetTrigger("harvestleft");
            }
        }

        //从网格上删除农作物
        gridPropertyDetails.seedItemCode = -1;
        gridPropertyDetails.growthDays = -1;
        gridPropertyDetails.daysSinceLastHarvest = -1;
        gridPropertyDetails.daysSinceWatered = -1;

        //作物是否应该在收获的动画之前被隐藏起来
        if (cropDetails.hideCropBeforeHarvestedAnimation)
        {
            GetComponentInChildren<SpriteRenderer>().enabled = false;
        }

        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX,gridPropertyDetails.gridY,gridPropertyDetails);

        if (cropDetails.isHarvestedAniamtion && animator!=null)
        {
            StartCoroutine(ProcessHarvestActionAfterAnimation(cropDetails, gridPropertyDetails, animator));
        }
        else
        {
            HarvestActions(cropDetails, gridPropertyDetails);
        }

    }

    private IEnumerator ProcessHarvestActionAfterAnimation(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails, Animator animator)
    {
        //Returns an AnimatorStateInfo with the information on the current state 返回一个当前动画状态信息 是否到达收获的动画状态 执行完状态继续向下执行
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Harvested"))
        {
            yield return null;
        }

        HarvestActions(cropDetails,gridPropertyDetails);
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