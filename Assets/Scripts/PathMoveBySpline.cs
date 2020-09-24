using UnityEngine;

/// <summary>
/// 通过Spline控制游动
/// </summary>
public class PathMoveBySpline : MonoBehaviour
{
    public Spline spline;
    public float passedTime = 0f;

    //游动速度
    public float speed = 0.02f;
    //角度偏移
    public float rotationOffset;
    //游动类型
    public WrapMode wrapMode = WrapMode.Loop;

    private Transform m_selfTrans;

    void Awake()
    {
        m_selfTrans = transform;
    }

    void Update()
    {
        //时间戳
        passedTime += Time.deltaTime * speed;
        //根据类型计算归一化时间戳
        float clampedParam = WrapValue(passedTime, 0f, 1f, wrapMode);
        //设置角度
        m_selfTrans.rotation = spline.GetOrientationOnSpline(WrapValue(passedTime + rotationOffset, 0f, 1f, wrapMode));
        //设置坐标
        m_selfTrans.position = spline.GetPositionOnSpline(clampedParam);
    }

    /// <summary>
    /// 根据类型计算归一化时间戳
    /// </summary>
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
