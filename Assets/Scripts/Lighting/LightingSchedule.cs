using UnityEngine;

[CreateAssetMenu(fileName = "lightingSchedule_",menuName = "Scriptable Objects/Lighting/LightingSchedule")]
public class LightingSchedule : ScriptableObject
{
  public LightingBrightness[] lightingBrightnessArray;
}
[System.Serializable]
public struct LightingBrightness
{
  public Season Season;
  public int hour;
  public float lightIntensity;
}
