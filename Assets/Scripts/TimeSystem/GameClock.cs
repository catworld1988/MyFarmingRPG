using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameClock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText = null;
    [SerializeField] private TextMeshProUGUI dateText = null;
    [SerializeField] private TextMeshProUGUI seasonText = null;
    [SerializeField] private TextMeshProUGUI yearText = null;

    private void OnEnable()
    {
        EventHandler.AdvanceGameMinuteEvent += UpdateGameTime;
    }

    private void OnDisable()
    {
        EventHandler.AdvanceGameMinuteEvent -= UpdateGameTime;
    }

    /// <summary>
    /// gameManager 传入广播中的数据
    /// </summary>
    /// <param name="gameYear"></param>
    /// <param name="gameSeason"></param>
    /// <param name="gameDay"></param>
    /// <param name="gameDayOfWeek"></param>
    /// <param name="gameHour"></param>
    /// <param name="gameMinute"></param>
    /// <param name="gameSecond"></param>
    private void UpdateGameTime(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute,
        int gameSecond)
    {
        //更新时间

        //gameMinute = gameMinute - (gameMinute % 10); //只显示10分钟的增量分钟 不会每一分钟都显示 最小化显示
        gameMinute = gameMinute - (gameMinute % 10);

        string ampm = "";
        string minute;

        if (gameHour >= 12)
        {
            ampm = "pm";
        }
        else
        {
            ampm = "am";
        }

        if (gameHour >= 13)
        {
            //转换下午时间
            gameHour -= 12;
        }

        if (gameMinute<10)
        {
            //格式化显示分钟
            minute = "0" + gameMinute.ToString();
        }
        else
        {
            minute = gameMinute.ToString();
        }

        minute = gameMinute.ToString();
        string time = gameHour.ToString() + " : " + minute + ampm;

        //输出UI
        timeText.SetText(time);
        dateText.SetText(gameDayOfWeek+". "+gameDay.ToString());
        seasonText.SetText(gameSeason.ToString());
        yearText.SetText("year "+ gameYear);
    }
}