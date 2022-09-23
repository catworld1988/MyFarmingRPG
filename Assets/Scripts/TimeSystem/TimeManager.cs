using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : SingletonMonobehaviour<TimeManager>,ISaveable
{
    private int gameYear = 1;
    private Season gameSeason = Season.Spring;
    private int gameDay = 1;
    private int gameHour = 6;
    private int gameMinute = 30;
    private int gameSecond = 0;
    private string gameDayOfWeek = "Mon";

    private bool gameClockPaused = false;

    private float gameTick = 0f;

    private string _isaveableUniqueID;
    public string ISaveableUniqueID { get=>_isaveableUniqueID; set=> _isaveableUniqueID=value; }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get=>_gameObjectSave; set=>_gameObjectSave=value; }


    protected override void Awake()
    {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void OnEnable()
    {
        ISaveableRegister();

        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnloadFadeOut;
        EventHandler.AfterSceneLoadEvent += AfterSceneLoadFadeIn;
    }



    private void OnDisable()
    {
        ISaveableDeregister();

        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnloadFadeOut;
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoadFadeIn;

    }


    private void BeforeSceneUnloadFadeOut()
    {
        gameClockPaused = true;
    }

    private void AfterSceneLoadFadeIn()
    {
        gameClockPaused = false;

    }
    private void Start()
    {
        EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }

    private void Update()
    {
        if (!gameClockPaused)
        {
            GameTick();
        }
    }

    private void GameTick()
    {
        gameTick += Time.deltaTime;
        if (gameTick >= Settings.secondsPerGameSecond)
        {
            //gameTick -= Time.deltaTime;
            gameTick -= Settings.secondsPerGameSecond;
            UpdateGameSecond();
        }
    }

    private void UpdateGameSecond()
    {
        gameSecond++;
        if (gameSecond > 59)
        {
            gameSecond = 0;
            gameMinute++;

            if (gameMinute > 59)
            {
                gameMinute = 0;
                gameHour++;

                if (gameHour > 23)
                {
                    gameHour = 0;
                    gameDay++;

                    if (gameDay > 30)
                    {
                        gameDay = 1;

                        int gs = (int)gameSeason;
                        gs++;

                        gameSeason = (Season)gs;

                        if (gs > 3)
                        {
                            gs = 0;
                            gameSeason = (Season)gs;

                            gameYear++;

                            if (gameYear>9999)
                            {
                                //UI只能处理4位数
                                gameYear = 1;
                            }
                            EventHandler.CallAdvanceGameYearEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                        }

                        EventHandler.CallAdvanceGameSeasonEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                    }

                    gameDayOfWeek = GetDayOfWeek();
                    EventHandler.CallAdvanceGameDayEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                }

                EventHandler.CallAdvanceGameHourEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
            }

            EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);

            //Debug.Log("gameYear: " + gameYear + "   gameSeason: " + gameSeason + "   gameDay: " + gameDay + "   gameHour: " + gameHour +"   gameMinute: " + gameMinute);
        }
        //Call to advance game second event would go here if required
    }

    /// <summary>
    /// 获取星期几
    /// </summary>
    private string GetDayOfWeek()
    {
        int totalDays = (((int)gameSeason) * 30) + gameDay;
        int dayOfWeek = totalDays % 7;

        switch (dayOfWeek)
        {
            case 1:
                return "Mon";

            case 2:
                return "Tue";

            case 3:
                return "Wed";

            case 4:
                return "Thu";

            case 5:
                return "Fri";

            case 6:
                return "Sat";

            case 0:
                return "Sun";

            default:
                return "";
        }
    }

    //TODO 删除
    /// <summary>
    /// 进阶 1分钟
    /// </summary>
    public void TestAdvanceGameMinute()
    {
        for (int i = 0; i < 60; i++)
        {
            UpdateGameSecond();
        }
    }

    /// <summary>
    /// 进阶 1天
    /// </summary>
    public void TestAdvanceGameGameDay()
    {
        for (int i = 0; i < 86400; i++)
         {
             UpdateGameSecond();
         }
    }
    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);

    }

    public GameObjectSave ISaveableSave()
    {
        //清除旧数据
        GameObjectSave.sceneDate.Remove(Settings.PersistentScene);
        //初始化变量
        SceneSave sceneSave = new SceneSave();
        sceneSave.intDictionary = new Dictionary<string, int>();
        sceneSave.stringDictionary = new Dictionary<string, string>();
        //添加需要保存的变量
        sceneSave.intDictionary.Add("gameYear",gameYear);
        sceneSave.intDictionary.Add("gameDay",gameDay);
        sceneSave.intDictionary.Add("gameHour",gameHour);
        sceneSave.intDictionary.Add("gameMinute",gameMinute);
        sceneSave.intDictionary.Add("gameSecond",gameSecond);

        sceneSave.stringDictionary.Add("gameDayOfWeek",gameDayOfWeek);
        sceneSave.stringDictionary.Add("gameSeason",gameSeason.ToString());

        //合在一起保存数据
        GameObjectSave.sceneDate.Add(Settings.PersistentScene,sceneSave);
        return GameObjectSave;

    }

    public void ISaveableLoad(GameSave gameSave)
    {
        //根据唯一标识 找到存储的时间数据
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID,out GameObjectSave gameObjectSave))
        {
            //获得场景数据
            GameObjectSave = gameObjectSave;
            if (gameObjectSave.sceneDate.TryGetValue(Settings.PersistentScene,out SceneSave sceneSave))
            {
                if (sceneSave.intDictionary!=null&& sceneSave.stringDictionary!=null)
                {
                    //int的时间数据
                    if (sceneSave.intDictionary.TryGetValue("gameYear",out int savedGameYear))
                    {
                        gameYear = savedGameYear;
                    }
                    if (sceneSave.intDictionary.TryGetValue("gameDay",out int savedGameDay))
                    {
                        gameDay = savedGameDay;
                    }
                    if (sceneSave.intDictionary.TryGetValue("gameHour",out int savedGameHour))
                    {
                        gameHour = savedGameHour;
                    }
                    if (sceneSave.intDictionary.TryGetValue("gameMinute",out int savedGameMinute))
                    {
                        gameMinute = savedGameMinute;
                    }
                    if (sceneSave.intDictionary.TryGetValue("gameSecond",out int savedGameSecond))
                    {
                        gameSecond = savedGameSecond;
                    }

                    //string的时间数据
                    if (sceneSave.stringDictionary.TryGetValue("gameDayOfWeek",out string savedGameDayOfWeek))
                    {
                        gameDayOfWeek = savedGameDayOfWeek;
                    }

                    if (sceneSave.stringDictionary.TryGetValue("gameSeason",out string savedGameSeason))
                    {
                        //字符串 转换成 季节的枚举数据
                        if (Enum.TryParse<Season>(savedGameSeason,out Season season))
                        {
                            gameSeason = season;
                        }
                    }

                    //初始化计时从0开始
                    gameTick = 0f;

                    //触发一下广播 更新时间
                    EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);

                }
            }
        }
    }

    public void ISaveableStoreScene(string sceneName)
    {
        //Nothing required here since Time Manager is running on the persistent scene
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        //Nothing required here since Time Manager is running on the persistent scene
    }
}