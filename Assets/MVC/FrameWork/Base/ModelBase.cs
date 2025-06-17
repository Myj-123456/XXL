using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 操作数据 处理数据 数据获取 数据推送
/// </summary>
public class ModelBase
{
    public UIWindow uiBase;
    public virtual void Init(UIWindow uiBase)
    {
        this.uiBase = uiBase;
    }
    public virtual void OnEnable()
    {

    }

    public virtual void OnDestory()
    {

    }
}
