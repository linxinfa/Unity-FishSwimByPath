using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunManager 
{
    public void Init(Camera uiCamera, Transform uiGunTrans)
    {
        var screenPos = uiCamera.WorldToScreenPoint(uiGunTrans.position);
        screenPos.z = GlobalDefines.BULLET_POS_Z;
        m_gunPos = Camera.main.ScreenToWorldPoint(screenPos);
    }


    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var screenPos = Input.mousePosition;
            screenPos.z = GlobalDefines.BULLET_POS_Z;
            var touchWorldPos = Camera.main.ScreenToWorldPoint(screenPos);
            BulletManager.instance.CreateBullet("Bullets/Bullet.prefab", m_gunPos, touchWorldPos);
        }
    }


    
    private Vector3 m_gunPos;

    private static GunManager s_instance;
    public static GunManager instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new GunManager();
            return s_instance;

        }
    }
}
