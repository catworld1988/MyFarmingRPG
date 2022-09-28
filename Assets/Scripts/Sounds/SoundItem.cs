using UnityEngine;

//Serializable 序列化的是可序列化的类或结构。
[System.Serializable]
public class SoundItem
{
    public SoundName soundName;
    public AudioClip soundClip;
    public string soundDescription;
    //音高最小值和最大值
    [Range(0.1f, 1.5f)] public float soundPitchRandomVariationMin = 0.8f;
    [Range(0.1f, 1.5f)] public float soundPitchRandomVariationMax = 1.2f;
    [Range(0f, 1f)] public float soundVolume = 1f;
}
