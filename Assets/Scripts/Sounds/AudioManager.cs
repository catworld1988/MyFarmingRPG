using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SingletonMonobehaviour<AudioManager>
{
    [SerializeField] private GameObject soundPrefab = null;
    [Header("Other")] [SerializeField] private SO_SoundList so_soundList = null;

    private Dictionary<SoundName, SoundItem> soundDictionary;

    protected override void Awake()
    {
        base.Awake();

        //初始化音频字典
        soundDictionary = new Dictionary<SoundName, SoundItem>();

        //音频数据:名称 音频参数转存入字典
        foreach (SoundItem soundItem in so_soundList.soundDetails)
        {
            soundDictionary.Add(soundItem.soundName, soundItem);
        }


    }

    public void PlaySound(SoundName soundName)
    {
        //检测是否存在 音频名字和预制体 存在out出参数
        if (soundDictionary.TryGetValue(soundName, out SoundItem soundItem) && soundPrefab != null)
        {
            GameObject soundGameObject = PoolManager.Instance.ReuseObject(soundPrefab, Vector3.zero, Quaternion.identity);

            Sound sound = soundGameObject.GetComponent<Sound>();

            //将参数装进预制体的sound组件里
            sound.SetSound(soundItem);
            //开启预制体
            soundGameObject.SetActive(true);
            //根据音频剪辑长度 关闭音频
            StartCoroutine(DisableSound(soundGameObject, soundItem.soundClip.length));

        }
    }

    private IEnumerator DisableSound(GameObject soundGameObject, float soundDuration)
    {
        yield return new WaitForSeconds(soundDuration);
        soundGameObject.SetActive(false);
    }
}

