using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

///// <summary>
///// 物品解析类 用来接收物品表每一行的内容
///// </summary>
//public class GoodData
//{
//    public int ID;
//    public string Name;
//    public int Type;
//    public float Hp;
//    public float Power;
//}

//public class FuBenData
//{
//    public int ID;
//    public Vector3 Position;
//}

public class NoneView : ViewBase
{
    Button bagBtn;
    //public static Dictionary<int, GoodData> goodsDic = new Dictionary<int, GoodData>();
    public override void Init(UIWindow uiBase)
    {
        base.Init(uiBase);
        //解析csv goodsDic保存所有的物品 key为物品id value为物品内容
        bagBtn = this.uiBase.transform.Find("BagBtn").GetComponent<Button>();
        AddButtonListener(bagBtn, () =>
        {
            UIManager.Instance.OpenWindow("BagPanel");
        });
    }

    //public Button btn1;
    //public Button btn2;

    //void Start()
    //{
    //    btn1.onClick.AddListener(() =>
    //    {
    //        List<FuBenData> list = new List<FuBenData>();
    //        //解析这个关卡（副本表）  list
    //        for (int i = 0; i < list.Count; i++)
    //        {
    //            var good = goodsDic[list[i].ID];
    //            var obj = GameObject.Instantiate(Resources.Load<GameObject>(good.Name));
    //            obj.transform.position = list[i].Position;
    //            GoodItemClass goodItemClass = obj.AddComponent<GoodItemClass>();
    //            goodItemClass.Init(good);
    //        }
    //    });
    //}
}

//public class GoodItemClass : MonoBehaviour
//{
//    GoodData data;
//    public void Init(GoodData data)
//    {
//        this.data = data;
//    }

//    void Update()
//    {

//    }
//}
