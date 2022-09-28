using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : SingletonMonobehaviour<AudioManager>
{
    [SerializeField] private GameObject soundPrefab = null;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambientSoundAudioSource = null;

    [SerializeField] private AudioSource gameMusicAudioSource = null;

    [Header("Audio Mixers")]
    [SerializeField] private AudioMixer gameAudioMixer = null;

    [Header("Audio Snapshots")]
    [SerializeField] private AudioMixerSnapshot gameMusicSnapshot = null;

    [SerializeField] private AudioMixerSnapshot gameAmbientSnapshot = null;

    [Header("Other")]
    [SerializeField] private SO_SoundList so_soundList = null;

    [SerializeField] private SO_SceneSoundsList so_SceneSoundsList = null;
    [SerializeField] private float defaultSceneMusicPlayTimeSeconds = 120f;
    [SerializeField] private float sceneMusicStartMinSecs = 20f;
    [SerializeField] private float sceneMusicStartMaxSecs = 40f;
    [SerializeField] private float musicTransitionSecs = 8f;

    private Dictionary<SoundName, SoundItem> soundDictionary;
    private Dictionary<SceneName, SceneSoundsItem> sceneSoundsDictionary;

    private Coroutine playSceneSoundsCoroutine;

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

        //初始化 场景音乐字典
        sceneSoundsDictionary = new Dictionary<SceneName, SceneSoundsItem>();

        //so_SceneSoundsList填充到字典
        foreach (SceneSoundsItem sceneSoundsItem in so_SceneSoundsList.SceneSoundsDetails)
        {
            sceneSoundsDictionary.Add(sceneSoundsItem.SceneName,sceneSoundsItem);
        }
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += playSceneSounds;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= playSceneSounds;
    }

    private void playSceneSounds()
    {
        //等待填充的变量
        SoundItem musicSoundItem = null;
        SoundItem ambientSoundItem = null;

        float musicPlayTime = defaultSceneMusicPlayTimeSeconds;

        //检测正确的场景
        if (Enum.TryParse<SceneName>(SceneManager.GetActiveScene().name,true,out SceneName currentSceneName))
        {
            //获取场景音乐和环境音
            if (sceneSoundsDictionary.TryGetValue(currentSceneName,out SceneSoundsItem sceneSoundsItem))
            {
                soundDictionary.TryGetValue(sceneSoundsItem.musicForScene, out musicSoundItem);
                soundDictionary.TryGetValue(sceneSoundsItem.ambientSoundForScene, out ambientSoundItem);
            }
            else
            {
                return;
            }

            //有场景声音的话 停止场景声音
            if (playSceneSoundsCoroutine !=null)
            {
                StopCoroutine(playSceneSoundsCoroutine);
            }

            playSceneSoundsCoroutine = StartCoroutine(PlaySceneSoundsCoroutine(musicPlayTime, musicSoundItem, ambientSoundItem));
        }
    }

    private IEnumerator PlaySceneSoundsCoroutine(float musicPlaySeconds, SoundItem musicSoundItem, SoundItem ambientSoundItem)
    {
        if (musicSoundItem !=null && ambientSoundItem !=null)
        {
            //开始环境声
            PlayAmbientSoundClip(ambientSoundItem, 0f);

            //随机等待几秒
            yield return new WaitForSeconds(UnityEngine.Random.Range(sceneMusicStartMinSecs, sceneMusicStartMaxSecs));

            //然后播放音乐
            PlayMusicSoundClip(musicSoundItem, musicTransitionSecs);

            //播放环境音前等待
            yield return new WaitForSeconds(musicPlaySeconds);

            //播放环境音乐剪辑
            PlayAmbientSoundClip(ambientSoundItem, musicTransitionSecs);
        }
    }

    private void PlayMusicSoundClip(SoundItem musicSoundItem, float transitionTimeSeconds)
    {
        //设置混音台 暴露出来的音乐音量参数 转换小数为分贝
        gameAudioMixer.SetFloat("MusicVolume", ConverSoundVolumeDecimalFractionToDecibels(musicSoundItem.soundVolume));

        //设置剪辑 并播放
        gameMusicAudioSource.clip = musicSoundItem.soundClip;
        gameMusicAudioSource.Play();

        // 过渡 按照时间差值 过渡到 混音台音乐快照
        gameMusicSnapshot.TransitionTo(transitionTimeSeconds);
    }

    private void PlayAmbientSoundClip(SoundItem ambientSoundItem, float transitionTimeSeconds)
    {
        //设置混音台 暴露出来的参数 转换小数为分贝
        gameAudioMixer.SetFloat("AmbientVolume", ConverSoundVolumeDecimalFractionToDecibels(ambientSoundItem.soundVolume));

        //设置剪辑 并播放
        ambientSoundAudioSource.clip = ambientSoundItem.soundClip;
        ambientSoundAudioSource.Play();

        //按照时间差值 过渡到 环境声快照
        gameAmbientSnapshot.TransitionTo(transitionTimeSeconds);
    }

    /// <summary>
    /// 转换小数到分贝 -80db to +20db之间
    /// </summary>
    private float ConverSoundVolumeDecimalFractionToDecibels(float volumDecimalFraction)
    {
        return (volumDecimalFraction * 100f - 80f);
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

