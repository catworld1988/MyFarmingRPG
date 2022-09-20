using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : SingletonMonobehaviour<VFXManager>
{
    private WaitForSeconds twoSeconds;
    [SerializeField] private GameObject deciduousLeavesFallingPrefab = null;
    [SerializeField] private GameObject PineConesFallingPrefab = null;
    [SerializeField] private GameObject reapingPrefab = null;
    [SerializeField] private GameObject ChoppingTreeTrunkPrefab = null;

    protected override void Awake()
    {
        base.Awake();

        twoSeconds = new WaitForSeconds(2f);
    }

    private void OnDisable()
    {
        EventHandler.HarvestActionEffectEvent -= displayHarvestActionEffect;
    }

    private void OnEnable()
    {
        EventHandler.HarvestActionEffectEvent += displayHarvestActionEffect;
    }


    private IEnumerator DisableHarvestActionEffect(GameObject effectGameObject, WaitForSeconds secondsToWait)
    {
        yield return secondsToWait;
        effectGameObject.SetActive(false);
    }

    private void displayHarvestActionEffect(Vector3 effectPosition, HarvestActionEffect harvestActionEffect)
    {
        switch (harvestActionEffect)
        {
            case HarvestActionEffect.deciduousLeavesFalling:
                //对象池中循环使用
                GameObject deciduousLeavesFalling = PoolManager.Instance.ReuseObject(deciduousLeavesFallingPrefab, effectPosition, Quaternion.identity);

                //激活显示
                deciduousLeavesFalling.SetActive(true);

                //两秒后关闭
                StartCoroutine(DisableHarvestActionEffect(deciduousLeavesFalling, twoSeconds));
                break;

            case HarvestActionEffect.pineConesFalling:
                GameObject pineConesFalling = PoolManager.Instance.ReuseObject(PineConesFallingPrefab, effectPosition, Quaternion.identity);

                pineConesFalling.SetActive(true);

                StartCoroutine(DisableHarvestActionEffect(pineConesFalling, twoSeconds));
                break;


            case HarvestActionEffect.choppingTreeTrunk:
                GameObject ChoppingTreeTrunk = PoolManager.Instance.ReuseObject(ChoppingTreeTrunkPrefab, effectPosition, Quaternion.identity);

                ChoppingTreeTrunk.SetActive(true);

                StartCoroutine(DisableHarvestActionEffect(ChoppingTreeTrunk, twoSeconds));
                break;


            case HarvestActionEffect.reaping:
                GameObject reaping = PoolManager.Instance.ReuseObject(reapingPrefab, effectPosition, Quaternion.identity);

                reaping.SetActive(true);

                StartCoroutine(DisableHarvestActionEffect(reaping, twoSeconds));
                break;


            case HarvestActionEffect.none:
                break;

            default:
                break;
        }
    }
}