using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 脚本挂载在UI上
/// </summary>
public class UIWindow : MonoBehaviour
{
    [HideInInspector]
    public string m_Name = ""; //ui名称
    public UIType m_Type; //ui类型
    public ShowType m_ShowType; //ui打开的类型
    public ModelBase model; //这个窗口的model脚本
    public ViewBase view; //这个窗口的view脚本
    public ControlBase control; //这个窗口的control脚本
    CanvasGroup canvasGroup;

    /// <summary>
    /// 初始化mvc三个脚本 相当于执行了mvc三个脚本的Awack
    /// </summary>
    public void Init()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            Debug.Log("当前UI没有挂载CanvasGroup脚本");
        if (model != null)
            model.Init(this);
        if (view != null) 
            view.Init(this);
        if (control != null)
            control.Init(this);
    }

    /// <summary>
    /// mvc三个脚本的OnEnable
    /// </summary>
    public void Enable()
    {
        if (model != null)
            model.OnEnable();
        if (view != null)
            view.OnEnable();
        if (control != null)
            control.OnEnable();
    }

    /// <summary>
    /// mvc三个脚本的OnDestory
    /// </summary>
    public void Destory()
    {
        if (model != null)
            model.OnDestory();
        if (view != null)
            view.OnDestory();
        if (control != null)
            control.OnDestory();
    }

    public void Update()
    {
        if (view != null)
            view.Update();
    }

    internal void Hide()
    {
        transform.name = m_Name + "_Hide";
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        Destory();
    }

    internal void Show()
    {
        transform.name = m_Name + "_Show";
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        Enable();
    }

    internal void OpenAsTop()
    {
        transform.SetAsLastSibling();
        Show();
    }
}
