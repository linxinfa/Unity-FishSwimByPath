using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源管理器
/// </summary>
public class ResourceMgr
{
    /// <summary>
    /// 实例化资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="resRelativePath">资源的相对路径</param>
    /// <returns>资源对象</returns>
    public T Instantiate<T>(string resRelativePath) where T : Object
    {
        var obj = LoadRes<T>(resRelativePath);
        if(null == obj)
        {
            Debug.LogError("Instantiate Res Error: " + resRelativePath);
            return null;
        }
        var t = typeof(T);
        if (typeof(Material) == t || typeof(AudioClip) == t)
        {
            //材质球或音效不需要实例化，直接GetComponent即可
            var go = obj as GameObject;
            return go.GetComponent<T>();
        }
        else
        {
            var go = Object.Instantiate(obj) as T;
            return go;
        }
    }

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="resRelativePath">资源的相对路径</param>
    /// <returns>资源对象</returns>
    private T LoadRes<T>(string resRelativePath) where T : Object
    {
        var assetPath = "Assets/GameRes/" + resRelativePath;
        if (resDic.ContainsKey(assetPath))
            return resDic[assetPath] as T;

        T res = null;
#if UNITY_EDITOR
        //编辑器下，直接从磁盘路径中读取资源
        res = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
        //TODO，通过AssetBundle加载

#endif
        if (null != res)
        {
            //将资源缓存，方便下次直接从缓存中取资源
            resDic[assetPath] = res;
        }
        return res;
    }

    private Dictionary<string, Object> resDic = new Dictionary<string, Object>();

    private static ResourceMgr s_instance;
    public static ResourceMgr instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new ResourceMgr();
            return s_instance;
        }
    }
}
