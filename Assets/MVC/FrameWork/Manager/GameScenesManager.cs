using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameScenesManager : UnitySingleton<GameScenesManager>
{
    Slider loadingSlider;  // 进度条 UI 元素
    GameObject loadingPanel;
    GameObject bootGame;
    Text loadingText;  // 显示加载进度的文本 UI 元素
    AsyncOperation asyncLoad = null;
    public CanvasGroup transitionCanvasGroup;  // 过渡场景的 CanvasGroup
    public float transitionDuration = 1f;  // 过渡时间
    public Dictionary<string, UnityEngine.Object> objDic = new Dictionary<string, UnityEngine.Object>();

    internal void Init(Boot boot)
    {
        bootGame = boot.transform.Find("LoadingRoot").gameObject;
        loadingPanel = boot.transform.Find("LoadingRoot/LoadingPanel").gameObject;
        transitionCanvasGroup = boot.transform.Find("LoadingRoot/Mask").GetComponent<CanvasGroup>();
        //loadingText = boot.transform.Find("LoadingRoot/LoadingPanel/Slider/Text (Legacy)").GetComponent<Text>();
        //loadingSlider = boot.transform.Find("LoadingRoot/LoadingPanel/Slider").GetComponent<Slider>();
        loadingPanel.SetActive(false);
    }

    /// <summary>
    /// 异步跳转场景
    /// </summary>
    /// <param name="sceneName">目标场景名称</param>
    /// <param name="panelName">目标场景打开的第一个界面 一般是背景</param>
    /// <param name="prePath">需要异步加载的资源</param>
    public void LoadSceneAsync(string sceneName, string panelName, string[] prePath = null)
    {
        bootGame.SetActive(true);
        StartCoroutine(LoadTransitionScene(sceneName, panelName, prePath));
    }

    // 预加载资源
    private IEnumerator PreloadResourcesAsync(string[] prePath)
    {
        if (prePath == null)
        {
            yield break;
        }
        // 遍历需要预加载的资源列表
        for (int i = 0; i < prePath.Length; i++)
        {
            yield return new WaitForEndOfFrame();
            string resourcePath = prePath[i];
            ResourceRequest resourceRequest = Resources.LoadAsync(resourcePath);
            while (!resourceRequest.isDone && resourceRequest.asset == null)
            {
                // 可选：可以根据资源加载的进度更新进度条
                loadingText.text = "正在预加载资源: " + resourcePath;
                loadingSlider.value = Mathf.Lerp(0f, 1f, resourceRequest.progress);
                yield return null;
            }

            // 资源加载完成
            Debug.Log("预加载资源完成: " + resourcePath);
            if (!objDic.ContainsKey(resourcePath))
                objDic.Add(resourcePath, resourceRequest.asset);
        }
    }

    // 加载过渡场景并处理过渡效果
    private IEnumerator LoadTransitionScene(string sceneName, string panelName, string[] prePath)
    {
        // 显示过渡场景并淡入
        yield return FadeInTransition();

        // 加载目标场景
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Empty");
        asyncLoad.allowSceneActivation = true;

        // 等待过渡场景加载完毕
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 开始加载目标场景
        yield return StartCoroutine(LoadGameSceneAsync(sceneName, panelName, prePath));
    }

    // 异步加载目标场景
    private IEnumerator LoadGameSceneAsync(string sceneName, string panelName, string[] prePath)
    {
        // 启动加载过程
        asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // 开始显示加载界面
        loadingPanel.gameObject.SetActive(true);

        while (!asyncLoad.isDone)
        {
            // 更新进度条

            // 加载进度达到0.9时，显示完成状态
            if (asyncLoad.progress >= 0.9f)
            {
                Resources.UnloadUnusedAssets();
                yield return PreloadResourcesAsync(prePath);
                //loadingSlider.value = 1;
                UIManager.Instance.CloseAllWindow();
                UIManager.Instance.OpenWindow(panelName);
                asyncLoad.allowSceneActivation = true;
            }
            else
            {
                //loadingSlider.value = asyncLoad.progress / 8f;
                //loadingText.text = "正在加载场景... " + (asyncLoad.progress * 100 / 8f).ToString("F0") + "%";
            }

            yield return null;
        }

        // 隐藏加载界面
        loadingPanel.gameObject.SetActive(false);

        // 淡出过渡效果
        yield return FadeOutTransition();
        bootGame.SetActive(false);
    }

    // 淡入过渡效果
    private IEnumerator FadeInTransition()
    {
        float timeElapsed = 0f;
        while (timeElapsed < transitionDuration)
        {
            transitionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timeElapsed / transitionDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transitionCanvasGroup.alpha = 1f;
    }

    // 淡出过渡效果
    private IEnumerator FadeOutTransition()
    {
        float timeElapsed = 0f;
        while (timeElapsed < transitionDuration)
        {
            transitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timeElapsed / transitionDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transitionCanvasGroup.alpha = 0f;
    }
}
