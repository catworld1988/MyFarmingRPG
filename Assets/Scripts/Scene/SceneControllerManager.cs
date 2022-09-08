using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneControllerManager : SingletonMonobehaviour<SceneControllerManager>
{
    private bool isFading;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private CanvasGroup faderCanvasGroup = null;
    [SerializeField] private Image faderImage = null;
    public SceneName startingSceneName;


    private IEnumerator Fade(float finalAlpha)
    {
        isFading = true;

        //阻挡 射线检测
        faderCanvasGroup.blocksRaycasts = true;

        //根据设定的时间计算出 淡入淡出所需的速度
        float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - finalAlpha) / fadeDuration;

        //近似值计算     循环计算 直到透明度接近最终透明度
        while (!Mathf.Approximately(faderCanvasGroup.alpha,finalAlpha))
        {
            //..move the alpha towards it's target alpha.移动当前透明度到它的目标透明度。
            faderCanvasGroup.alpha = Mathf.MoveTowards(faderCanvasGroup.alpha, finalAlpha, fadeSpeed * Time.deltaTime);

            //Wait for a frame then continue
            yield return null;
        }

        //淡入淡出结束 重置淡入淡出状态为 不在进行
        isFading = false;

        //开启射线检测
        faderCanvasGroup.blocksRaycasts = false;
    }



    //This is the coroutine where the 'building blocks' of the script are put together.
    //这是将脚本的“构建块”组合在一起的协程。
    private IEnumerator FadeAndSwitchScenes(string sceneName, Vector3 spawnPosition)
    {
        //呼叫场景前 广播
        EventHandler.CallBeforeSceneUnloadFadeOutEvent();
        //开始淡出到黑色
        yield return StartCoroutine(Fade(1f));

        //##存储场景数据 ##
        SaveLoadManager.Instance.StoreCurrentSceneDate();


        //设置玩家位置
        Player.Instance.gameObject.transform.position = spawnPosition;

        //呼叫 卸载场景前淡出 广播
        EventHandler.CallBeforeSceneUnloadEvent();
        //卸载当前场景
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        //开始加载场景等待完成
        yield return StartCoroutine(LoadSceneAndSetActive(sceneName));

        //呼叫 加载场景后 广播
        EventHandler.CallAfterSceneLoadEvent();


        //##读取场景数据 ##
        SaveLoadManager.Instance.RestoreCurrentSceneDate();


        //开始淡入 并 等待完成
        yield return StartCoroutine(Fade(0f));

        //呼叫 加载场景后淡入 广播
        EventHandler.CallAfterSceneLoadFadeInEvent();
    }


    private IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        //Allow the given scene to load over several frames and add it to the already loaded scenes (just the Persistent scene at this point
        //允许在多个帧上加载给定的场景，并将其添加到已加载的场景中(此时仅为持久场景)
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        //Find the scene that was most recently loaded (the one at the last index of the loaded scenes)
        //查找最近加载的场景（加载场景最后索引中的场景）
        Scene newlyLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        //Set the newly loaded scene as the active scene (this marks it as the one to be unloaded next).
        //将新加载的场景设置为活动场景（这将其标记为下要卸载的场景）。
        SceneManager.SetActiveScene(newlyLoadedScene);
    }

    private IEnumerator Start()
    {
        //Set the initial alpha to start off with a black screen.
        //将初始Alpha设置为从黑屏开始。
        faderImage.color = new Color(0f, 0f, 0f, 1f);
        faderCanvasGroup.alpha = 1f;

        //Start the first scene loading and wait for it to finish.
        //开始加载第一个场景并等待其完成。
        yield return StartCoroutine(LoadSceneAndSetActive(startingSceneName.ToString()));

        //如果有订阅者，通知它们
        EventHandler.CallAfterSceneLoadEvent();


        //##读取场景数据 ##
        SaveLoadManager.Instance.RestoreCurrentSceneDate();


        //Once the scene is finished loading,start fading in.
        //场景加载完成后，开始淡入。
        StartCoroutine(Fade(0f));
    }


    //This is the main external point of contact and influence from the rest of the project.这是项目其余部分的主要外部调用点。
    //This will be called when the player wants to switch scenes.当玩家想要切换场景时就会调用这个函数。
    public void FadeAndLoadScene(string sceneName, Vector3 spawnPosition)
    {
        //If a fade isn't happening then start fading and switching scenes.
        //如果没有发生淡入淡出，则开始淡入淡出并切换场景。
        if (!isFading)
        {
            StartCoroutine(FadeAndSwitchScenes(sceneName, spawnPosition));
        }
    }


}
