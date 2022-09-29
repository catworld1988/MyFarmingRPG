using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightingControl : MonoBehaviour
{
    [SerializeField] private LightingSchedule lightingSchedule;
    [SerializeField] private bool isLightFlicker = false;
    [SerializeField] [Range(0f, 1f)] private float lightFlickerIntensity;
    [SerializeField] [Range(0f, 0.2f)] private float lightFlickerTimeMin;
    [SerializeField] [Range(0f, 0.2f)] private float lightFlickerTimeMax;

    private Light2D light2D;
    private Dictionary<string, float> lightingBrightnessDictionary = new Dictionary<string, float>();
    private float currentLightIntensity;
    private float lightFlickerTimer = 0f;
    private Coroutine fadeInLightRoutine;

    private void Awake()
    {
        //获取light2D
        light2D = GetComponentInChildren<Light2D>();

        //如果没有light2D 就关闭
        if (light2D== null)
        {
            enabled = false;
        }

        //填充灯光字典
        foreach (LightingBrightness lightingBrightness in lightingSchedule.lightingBrightnessArray)
        {
            //KEY:季节几点
            string key = lightingBrightness.Season.ToString() + lightingBrightness.hour.ToString();

            lightingBrightnessDictionary.Add(key,lightingBrightness.lightIntensity);
        }
    }

    private void OnEnable()
    {
        //订阅事件
        EventHandler.AfterSceneLoadEvent += EventHandler_AfterSceneLoadEvent;
        EventHandler.AdvanceGameHourEvent += EventHandler_AdvanceGameHourEvent;
    }

    private void OnDisable()
    {
        //取消订阅事件
        EventHandler.AfterSceneLoadEvent -= EventHandler_AfterSceneLoadEvent;
        EventHandler.AdvanceGameHourEvent -= EventHandler_AdvanceGameHourEvent;
    }


    /// <summary>
    /// 游戏时间 小时触发的函数
    /// </summary>
    private void EventHandler_AdvanceGameHourEvent(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute,
        int gameSecond)
    {
        SetLightingIntensity(gameSeason, gameHour, true);
    }

    /// <summary>
    /// 场景加载后调用函数
    /// </summary>
    private void EventHandler_AfterSceneLoadEvent()
    {
        SetLightAfterSceneLoaded();
    }

    /// <summary>
    /// 触发灯光闪烁
    /// </summary>
    private void Update()
    {
        if (isLightFlicker)
        {
            //灯光闪烁倒计时
            lightFlickerTimer -= Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        if (lightFlickerTimer <= 0f && isLightFlicker)
        {
            LightFlicker();
        }
        else
        {
            light2D.intensity = currentLightIntensity;
        }
    }

    private void SetLightAfterSceneLoaded()
    {
        Season gameSeason = TimeManager.Instance.GetSeason();
        int gameHour = TimeManager.Instance.GetGameTime().Hours;

        //Set light intensity immediately without fading in 设置灯光强度淡出
        SetLightingIntensity(gameSeason, gameHour,false);
    }

    /// <summary>
    /// 根据季节和时间 设置灯光亮度
    /// </summary>
    private void SetLightingIntensity(Season gameSeason, int gameHour, bool fadein)
    {
        int i = 0;

        while (i<=23)
        {
            //字典键值
            string key = gameSeason.ToString() + gameHour.ToString();

            if (lightingBrightnessDictionary.TryGetValue(key, out float targetLightingIntensity))
            {
                if (fadein)
                {
                    //停止已有的淡入
                    if (fadeInLightRoutine != null) StopCoroutine(fadeInLightRoutine);

                    //淡入到新的灯光强度
                    fadeInLightRoutine = StartCoroutine(FadeInLightRoutine(targetLightingIntensity));
                }
                else
                {
                    currentLightIntensity = targetLightingIntensity;
                }
                break;
            }

            i++;

            gameHour--;

            if (gameHour < 0)
            {
                gameHour = 23;
            }
        }
    }

    private IEnumerator FadeInLightRoutine(float targetLightingIntensity)
    {
        float fadeDuration = 5f;

        //计算 淡入速度= 距离/时间
        float fadeSpeed = Mathf.Abs(currentLightIntensity - targetLightingIntensity) / fadeDuration;

        //当前灯强 和目标灯强 不大月相等 就循环过渡
        while (!Mathf.Approximately(currentLightIntensity,targetLightingIntensity))
        {
            //使灯光强度 向目标强度移动
            currentLightIntensity = Mathf.MoveTowards(currentLightIntensity, targetLightingIntensity, fadeSpeed * Time.deltaTime);

            yield return null;
        }

        currentLightIntensity = targetLightingIntensity;

    }

    private void LightFlicker()
    {
        //计算一个随机闪烁强度
        light2D.intensity = Random.Range(currentLightIntensity, currentLightIntensity + (currentLightIntensity * lightFlickerIntensity));

        //计算一个随机 灯光闪烁计时数字
        lightFlickerTimer = Random.Range(lightFlickerTimeMin, lightFlickerTimeMax);
    }
}
