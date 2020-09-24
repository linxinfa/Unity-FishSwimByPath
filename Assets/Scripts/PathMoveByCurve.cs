using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMoveByCurve 
{
    private Transform m_trans;

    private Curve m_curve;

    public WrapMode wrapMode = WrapMode.Loop;

    public float speed = 0.02f;

    public float passedTime = 0f;

    public float rotationOffset;

    Vector3 m_lastPosOnCurve = Vector3.zero;

    public void Init(Transform transform, Curve curve)
    {
        m_trans = transform;
        m_curve = curve;
    }

    public void Update()
    {
        passedTime += Time.deltaTime * speed;
        float clampedParam = WrapValue(passedTime, 0f, 1f, wrapMode);

        //坐标
        m_trans.position = m_curve.GetPositionOnCurve(clampedParam);
        //旋转
        var forward = m_trans.position - m_lastPosOnCurve;
        var newRot = Quaternion.LookRotation(forward, new Vector3(0, 1, 0));
        m_trans.localRotation = Quaternion.Slerp(m_trans.localRotation, newRot, passedTime * 5);

        m_lastPosOnCurve = m_trans.position;
    }

    private float WrapValue(float v, float start, float end, WrapMode wMode)
    {
        switch (wMode)
        {
            case WrapMode.Clamp:
            case WrapMode.ClampForever:
                return Mathf.Clamp(v, start, end);
            case WrapMode.Default:
            case WrapMode.Loop:
                return Mathf.Repeat(v, end - start) + start;
            case WrapMode.PingPong:
                return Mathf.PingPong(v, end - start) + start;
            default:
                return v;
        }
    }
}
