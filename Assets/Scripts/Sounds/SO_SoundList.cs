using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "so_SoundList" ,menuName = "Scriptable Objects/Sounds/Sound List")]
public class SO_SoundList : ScriptableObject
{
    //SerializeField是强制对私有字段序列化
    [SerializeField]
    public List<SoundItem> soundDetails;
}
