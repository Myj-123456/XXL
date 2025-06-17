using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 资源加载框架 包含加载、获取、卸载
/// 加载：同步加载，异步加载，预加载
/// 对象池
/// </summary>
public class ResourceMgr : Singleton<ResourceMgr>
{
    Dictionary<string, Object> assetDic = new Dictionary<string, Object>();

    /// <summary>
    /// Resources泛型加载资源方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T ResLoadAsset<T>(string path) where T : Object
    {
        //如果路径为空 则直接return null
        if (string.IsNullOrEmpty(path))
            return null;
        Object obj = null;//生命一个接收对象 
        if (assetDic.TryGetValue(path, out obj)) //通过路径在字典中找到值 value
        {
            return obj as T;
        }
        obj = Resources.Load<T>(path);
        assetDic.Add(path, obj);
        return obj as T;
    }
    /// <summary>
    /// AssetDataBase泛型加载资源方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T AssetDataBaseLoadAsset<T>(string path) where T : Object
    {
        //如果路径为空 则直接return null
        if (string.IsNullOrEmpty(path))
            return null;
        Object obj = null;//生命一个接收对象 
        if (assetDic.TryGetValue(path, out obj)) //通过路径在字典中找到值 value
        {
            return obj as T;
        }
#if UNITY_EDITOR
        obj = AssetDatabase.LoadAssetAtPath<T>(path);
        assetDic.Add(path, obj);
        return obj as T;
#endif
        return null;
    }
}
