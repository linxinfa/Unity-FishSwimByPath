using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishManager
{
    /// <summary>
    /// 创建鱼接口
    /// </summary>
    /// <param name="fishRes">鱼资源</param>
    /// <param name="pathRes">路径资源</param>
    public void CreateFish(string fishRes, string pathRes)
    {
        var uuid = System.Guid.NewGuid().ToString("N");
        var fishObj = new GameObject("fish");
        //加载路径二进制文件，反序列化成Curve对象
        var curve = PathMgr.instance.GetPathSplineByBin(pathRes);

        var moveCtrler = new PathMoveByCurve();
        moveCtrler.Init(fishObj.transform, curve);
        //鱼模型
        var model = ResourceMgr.instance.Instantiate<GameObject>(fishRes);
        model.transform.SetParent(fishObj.transform, false);
        //封装成FishUnit
        FishUnit unit = new FishUnit(uuid, fishObj, moveCtrler);

        //加入到容器中
        m_fishDic.Add(unit.uuid, unit);
    }

    /// <summary>
    /// 销毁某条鱼
    /// </summary>
    /// <param name="uuid"></param>
    public void DestroyFish(string uuid)
    {
        if(m_fishDic.ContainsKey(uuid))
        {
            m_fishDic[uuid].Destroy();
            m_fishDic.Remove(uuid);
        }
    }

    /// <summary>
    /// 每帧更新，通过Main脚本调用
    /// </summary>
    public void Update()
    {
        foreach(var unit in m_fishDic.Values)
        {
            unit.Update();
        }
    }

    Dictionary<string, FishUnit> m_fishDic = new Dictionary<string, FishUnit>();

    private static FishManager s_instance;
    public static FishManager instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new FishManager();
            return s_instance;
        }
    }
}
