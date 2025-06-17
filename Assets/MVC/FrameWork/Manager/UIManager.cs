using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// ui跟随父节点的类型
/// </summary>
public enum UIType
{
    NULL,//没有设置则为默认
    TIP,//弹窗
    NORMAL,//普通
    BACKGROUND,//背景
}

/// <summary>
/// ui打开的类型
/// </summary>
public enum ShowType
{
    NULL,//没有设置则为默认
    TIP,//弹窗
    NORMAL,//普通，可重复弹出
    EXCLUSIVE,//互斥
    BACKGROUND,//一直存在
}

public class UIManager : Singleton<UIManager>
{
    string uiPrefabPath = "UIPrefab/";//预制体路径
    Transform tipsRoot, normalRoot, backRoot;//三个父节点
    public Dictionary<string, UIWindow> allUIDic = new Dictionary<string, UIWindow>();//所有ui字典
    public Dictionary<string, UIWindow> poolDic = new Dictionary<string, UIWindow>();//互斥列表字典
    UIWindow exclusivePanel;//互斥窗口保存

    /// <summary>
    /// 初始化ui框架 初始化ui跟随的父节点
    /// </summary>
    public void Init(Transform tipsRoot, Transform normalRoot, Transform backRoot)
    {
        this.tipsRoot = tipsRoot;
        this.normalRoot = normalRoot;
        this.backRoot = backRoot;
    }

    /// <summary>
    /// 打开界面的方法
    /// </summary>
    /// <param name="uiName">需要打开界面的名称</param>
    /// <returns></returns>
    public UIWindow OpenWindow(string uiName)
    {
        UIWindow uiBase = GetWindowFunc(uiName);
        if (uiBase == null)
        {
            Debug.Log("界面加载失败");
            return null;
        }
        else
        {
            //根据类型显示
            switch (uiBase.m_ShowType)
            {
                case ShowType.NULL:
                    NormalOpenFunc(uiBase);
                    break;
                case ShowType.TIP:
                    uiBase.OpenAsTop();
                    break;
                case ShowType.NORMAL:
                    NormalOpenFunc(uiBase);
                    break;
                case ShowType.EXCLUSIVE:
                    ExclusiveOpenFunc(uiBase);
                    break;
                case ShowType.BACKGROUND:
                    uiBase.Show();
                    break;
                default:
                    break;
            }
            return uiBase;
        }
    }

    /// <summary>
    /// 互斥打开方式
    /// </summary>
    /// <param name="uiBase"></param>
    private void ExclusiveOpenFunc(UIWindow uiBase)
    {
        exclusivePanel = uiBase;
        //将所有界面都关闭
        foreach (var panel in poolDic.Values)
        {
            panel.Hide();
        }
        exclusivePanel.Show();
    }

    /// <summary>
    /// 普通窗口打开逻辑
    /// </summary>
    /// <param name="uiBase"></param>
    private void NormalOpenFunc(UIWindow uiBase)
    {
        //判断有没有互斥窗口 有的话关闭互斥窗口
        if (exclusivePanel != null)
        {
            exclusivePanel.Hide();
        }
        //判断池里有没有这个界面 如果没有 就添加上 方便后续互斥窗口打开的时候 将池里所有的界面都关闭
        if (!poolDic.ContainsKey(uiBase.m_Name))
        {
            poolDic.Add(uiBase.m_Name, uiBase);
        }
        //将界面打开
        uiBase.Show();
    }

    /// <summary>
    /// 获取ui界面
    /// </summary>
    /// <param name="uiName"></param>
    /// <returns></returns>
    private UIWindow GetWindowFunc(string uiName)
    {
        UIWindow uiBase;
        #region 第一种编写方式
        //if(!allUIDic.TryGetValue(uiName,out uiBase))
        //{
        //    string name = uiName.Replace("Panel", "");
        //    GameObject uiPrefab = Resources.Load<GameObject>(uiPrefabPath + uiName);
        //    if (uiPrefab == null)
        //    {
        //        Debug.Log("资源里没有" + uiName + "这个资源");
        //        return null;
        //    }
        //    uiBase = uiPrefab.GetComponent<UIBase>();
        //    if (uiBase == null)
        //    {
        //        Debug.Log("资源没有挂载UIBase脚本");
        //        return null;
        //    }
        //    uiPrefab = InstantiateUIPrefab(uiPrefab, uiBase.m_Type);
        //    uiBase = uiPrefab.GetComponent<UIBase>();
        //    uiBase.m_Name = uiName;
        //    Type model = Type.GetType(name + "Model");
        //    if (model != null)
        //    {
        //        uiBase.model = Activator.CreateInstance(model) as ModelBase;
        //    }
        //    else
        //    {
        //        Debug.Log("资源没有对应的Model脚本");
        //    }
        //    Type view = Type.GetType(name + "View");
        //    if (view != null)
        //    {
        //        uiBase.view = Activator.CreateInstance(view) as ViewBase;
        //    }
        //    else
        //    {
        //        Debug.Log("资源没有对应的View脚本");
        //    }
        //    Type control = Type.GetType(name + "Control");
        //    if (control != null)
        //    {
        //        uiBase.control = Activator.CreateInstance(control) as ControlBase;
        //    }
        //    else
        //    {
        //        Debug.Log("资源没有对应的Control脚本");
        //    }
        //    uiBase.Init();
        //    allUIDic.Add(uiName, uiBase);
        //}
        //else
        //{
        //    //说明这个界面已经打开过了
        //}
        #endregion
        if (allUIDic.ContainsKey(uiName))
        {
            uiBase = allUIDic[uiName];
            //说明多次打开过了
        }
        else
        {
            ///获取这个ui的名字 如果是GamePanel 得到的结果是Game
            string name = uiName.Replace("Panel", "");
            GameObject uiPrefab = ResourceMgr.Instance.ResLoadAsset<GameObject>(uiPrefabPath + uiName);// Resources.Load<GameObject>(uiPrefabPath + uiName);
            if (uiPrefab == null)
            {
                Debug.Log("资源里没有" + uiName + "这个资源");
                return null;
            }
            uiBase = uiPrefab.GetComponent<UIWindow>();
            if (uiBase == null)
            {
                Debug.Log(uiName+ "资源没有挂载UIWindow脚本");
                return null;
            }
            uiPrefab = InstantiateUIPrefab(uiPrefab, uiBase.m_Type);
            uiBase = uiPrefab.GetComponent<UIWindow>();
            uiBase.m_Name = uiName;
            Type model = Type.GetType(name + "Model");
            if (model != null)
            {
                uiBase.model = Activator.CreateInstance(model) as ModelBase;
            }
            else
            {
                Debug.Log(uiName + "资源没有对应的Model脚本");
            }
            Type view = Type.GetType(name + "View");
            if (view != null)
            {
                uiBase.view = Activator.CreateInstance(view) as ViewBase;
            }
            else
            {
                Debug.Log(uiName + "资源没有对应的View脚本");
            }
            Type control = Type.GetType(name + "Control");
            if (control != null)
            {
                uiBase.control = Activator.CreateInstance(control) as ControlBase;
            }
            else
            {
                Debug.Log(uiName + "资源没有对应的Control脚本");
            }
            uiBase.Init();
            allUIDic.Add(uiName, uiBase);
        }
        return uiBase;
    }

    private GameObject InstantiateUIPrefab(GameObject uiPrefab, UIType m_Type)
    {
        Debug.Log(uiPrefab.name + "类型为" + m_Type.ToString());
        if (uiPrefab == null)
            return null;
        switch (m_Type)
        {
            case UIType.NULL:
                return GameObject.Instantiate(uiPrefab, normalRoot);
            case UIType.TIP:
                return GameObject.Instantiate(uiPrefab, tipsRoot);
            case UIType.NORMAL:
                return GameObject.Instantiate(uiPrefab, normalRoot);
            case UIType.BACKGROUND:
                return GameObject.Instantiate(uiPrefab, backRoot);
            default:
                return null;
        }
    }

    public void CloseAllWindow()
    {
        foreach(var item in allUIDic.Values)
        {
            item.Hide();
        }
    }

    public void CloseWindow(string uiName)
    {
        if (!allUIDic.ContainsKey(uiName)) return;
        switch (allUIDic[uiName].m_ShowType)
        {
            case ShowType.NULL:
                NormalCloseFunc(allUIDic[uiName]);
                break;
            case ShowType.TIP:
                break;
            case ShowType.NORMAL:
                NormalCloseFunc(allUIDic[uiName]);
                break;
            case ShowType.EXCLUSIVE:
                ExclusiveCloseFunc(allUIDic[uiName]);
                break;
            case ShowType.BACKGROUND:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 根据view关闭界面
    /// </summary>
    public void CloseWidnow(UIWindow uiBase)
    {
        switch (uiBase.m_ShowType)
        {
            case ShowType.NULL:
                NormalCloseFunc(uiBase);
                break;
            case ShowType.TIP:
                uiBase.Hide();
                break;
            case ShowType.NORMAL:
                NormalCloseFunc(uiBase);
                break;
            case ShowType.EXCLUSIVE:
                ExclusiveCloseFunc(uiBase);
                break;
            case ShowType.BACKGROUND:
                uiBase.Hide();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 互斥窗口关闭方法
    /// </summary>
    /// <param name="uiBase"></param>
    private void ExclusiveCloseFunc(UIWindow uiBase)
    {
        if (poolDic.Count > 0)
        {
            foreach (var panel in poolDic.Values)
            {
                panel.Show();
            }
        }
        exclusivePanel.Hide();
        exclusivePanel = null;
    }

    /// <summary>
    /// 普通界面关闭的方法
    /// </summary>
    /// <param name="uiBase"></param>
    private void NormalCloseFunc(UIWindow uiBase)
    {
        uiBase.Hide();
        if(poolDic.ContainsKey(uiBase.m_Name))
        {
            poolDic.Remove(uiBase.m_Name);
        }
        if (exclusivePanel != null)
            exclusivePanel.Show();
    }

    /// <summary>
    /// 获取UI
    /// </summary>
    /// <param name="uiName"></param>
    /// <returns></returns>
    public UIWindow GetWindow(string uiName)
    {
        if (allUIDic.ContainsKey(uiName))
        {
            return allUIDic[uiName];
        }
        else
        {
            Debug.Log("场景中没有加载过" + uiName);
            return null;
        }
    }
}
