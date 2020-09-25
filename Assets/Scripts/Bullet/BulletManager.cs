using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager 
{
    public void Init()
    {
        if (null == m_rootTrans)
        {
            m_rootTrans = new GameObject("BulletRoot").transform;
        }

        s_screenLeftBottom = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, GlobalDefines.BULLET_POS_Z));
        s_screenRightTop = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, GlobalDefines.BULLET_POS_Z));
    }

    public void CreateBullet(string resPath, Vector3 startPos, Vector3 touchPos)
    {
        var uuid = System.Guid.NewGuid().ToString("N");
        var obj = ResourceMgr.instance.Instantiate<GameObject>(resPath);
        obj.transform.SetParent(m_rootTrans, false);
        
        obj.transform.position = startPos;
        var direction = (touchPos - startPos).normalized;
        obj.transform.forward = direction;
        BulletUnit unit = new BulletUnit(uuid, obj, direction, 30);

        //加入到容器中
        m_bulletDic.Add(unit.uuid, unit);
    }

    public void Update()
    {
        foreach (var unit in m_bulletDic.Values)
        {
            unit.Update();
        }
    }



    public static void CalculateBulletPos(Vector3 curPos, ref Vector3 nextPos, ref Vector3 velocity)
    {
        if(nextPos.y > curPos.y)
        {
            if(nextPos.y > s_screenRightTop.y)
            {
                nextPos.y = s_screenRightTop.y;
                velocity.y = -velocity.y;
            }
        }
        else
        {
            if(nextPos.y < s_screenLeftBottom.y)
            {
                nextPos.y = s_screenLeftBottom.y;
                velocity.y = -velocity.y;
            }
        }

        if(nextPos.x > curPos.x)
        {
            if (nextPos.x > s_screenRightTop.x)
            {
                nextPos.x = s_screenRightTop.x;
                velocity.x = -velocity.x;
            }
        }
        else
        {
            if (nextPos.x < s_screenLeftBottom.x)
            {
                nextPos.x = s_screenLeftBottom.x;
                velocity.x = -velocity.x;
            }
        }
    }

    
    private static Vector3 s_screenLeftBottom;
    private static Vector3 s_screenRightTop;
    private Transform m_rootTrans;

    private Transform m_uiGunTrans;
    

    private Dictionary<string, BulletUnit> m_bulletDic = new Dictionary<string, BulletUnit>();

    private static BulletManager s_instance;
    public static BulletManager instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new BulletManager();
            return s_instance;
        }
    }
}
