using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletUnit 
{
    public string uuid { get; private set; }
    public GameObject obj { get; private set; }
    public Transform trans { get; private set; }

    /// <summary>
    /// 飞行方向
    /// </summary>
    public Vector3 direction;
    /// <summary>
    /// 速度
    /// </summary>
    public float speed;

    private float m_timer;
    private const int MAX_TIME_LIMIT = 8;

    public BulletUnit(string uuid, GameObject obj, Vector3 direction, float speed)
    {
        this.uuid = uuid;
        this.obj = obj;
        this.trans = obj.transform;
        this.direction = direction;
        this.speed = speed;
    }

    public void Update()
    {
        var deltaTime = Time.deltaTime;
        m_timer += deltaTime;
        MoveBullet(deltaTime);
        if (m_timer >= MAX_TIME_LIMIT)
        {
            Destroy();
        }
    }

    public void Destroy()
    {
        //TODO做对象池回收
    }

    private void MoveBullet(float deltaTime)
    {
        var curPos = trans.position;
        var nextPos = curPos + direction * deltaTime* speed;
        BulletManager.CalculateBulletPos(curPos, ref nextPos, ref direction);
        trans.position = nextPos;
        trans.up = Vector3.forward;
        trans.forward = direction;
        
    }
}
