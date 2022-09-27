using UnityEngine;

public class GameManager : SingletonMonobehaviour<GameManager>
{

    public Weather currentWeather;
    protected override void Awake()
    {
        base.Awake();

        //TODO 需要一个分辨率设置选项页面
        Screen.SetResolution(1920,1080,FullScreenMode.FullScreenWindow,0);

        //设置开始天气
        currentWeather = Weather.dry;
    }
}
