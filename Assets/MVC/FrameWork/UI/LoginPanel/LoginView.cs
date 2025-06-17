using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginView : ViewBase
{
    Button loginBtn;
    public override void Init(UIWindow uiBase)
    {
        base.Init(uiBase);
        loginBtn = uiBase.transform.Find("LoginBtn").GetComponent<Button>();
        loginBtn.onClick.AddListener(() =>
        {
            GameScenesManager.Instance.LoadSceneAsync("Game", "GamePanel");
        });
    }
}
