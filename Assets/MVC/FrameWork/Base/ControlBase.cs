using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 是View和Model的中间调节者 view让我执行什么操作 我操作的数据是model里面的
/// </summary>
public class ControlBase
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
