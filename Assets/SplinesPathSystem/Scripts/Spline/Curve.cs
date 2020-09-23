using UnityEngine;


/**
 * This Curve class is defined by a sequence of control points.
 **/
public class Curve
{
    /// <summary>
    /// 曲线计算的插值方法
    /// </summary>
    public enum InterpolationMode
    {
        Hermite = 0, 				///< Hermite Spline
		Bezier = 1, 				///< Bézier Spline
		BSpline = 2, 				///< B-Spline
		Linear = 3,					///< Linear Interpolation
		CustomMatrix = 4			///< Use a custom coefficient matrix for interpolation (if CustomMatrix hasn't been assigned to, the hermite matrix will be used) 
	}

    public Curve(int interpolateAccracy = 5)
    {
        curveLength = new CurveLength();
        this.interpolateAccracy = interpolateAccracy;
    }

    public Curve(bool autoClose, InterpolationMode interpolationMode, Vector3[] wayPoints, int interpolateAccracy)
    {
        curveLength = new CurveLength();
        this.interpolateAccracy = interpolateAccracy;
        this.autoClose = autoClose;
        this.interpolationMode = interpolationMode;
        this.SetPoints(wayPoints, false);
    }

    public Vector3[] m_controlNodesPosition;
    //private int m_subSegPosIndex;
    private bool m_autoClose = false;

    /// 优化的代码，预先记录已经取的点
    private int m_last_index = int.MinValue;
    Vector3 p0, p1, p2, p3;

    /// <summary>
    /// 路径计算的插值方法
    /// </summary>
    public InterpolationMode interpolationMode
    {
        get { return m_interpolationMode; }
        set { m_interpolationMode = value; }
    }

    /// <summary>
    /// 路径计算的插值方法
    /// </summary>
    private InterpolationMode m_interpolationMode = InterpolationMode.BSpline;

    public bool autoClose
    {
        get { return m_autoClose; }
        set { m_autoClose = value; }
    }

    public int controlNodeCount
    {
        get { return m_autoClose ? m_controlNodesPosition.Length + 1 : m_controlNodesPosition.Length; }
    }

    public int segmentCount
    {
        get { return Mathf.Max((controlNodeCount - 1), 0); }
    }

    public int interpolateAccracy;

    public CurveLength curveLength;

    /// <summary>
    /// 设定曲线上的控制点，设定后会计算细分点的与曲线的长度数据，比较大的运算量
    /// 要修改autoClose要在这个函数之前调用
    /// </summary>
    /// <param name="points">曲线上的控制点, 不得少于2个点</param>
    /// <param name="isClonePoints">是否要clone points这个数组， 默认是false</param>
    /// <param name="cvlen">曲线长模块，如果不传入，则会自己计算， 默认没有</param>
    public void SetPoints(Vector3[] points, bool isClonePoints = false, CurveLength cvlen = null)
    {
        if (points.Length < 2)
        {
            Debug.LogError("Curves:SetPoints: points.Length < 2");
            return;
        }

        m_controlNodesPosition = isClonePoints ? ((Vector3[])points.Clone()) : points;
        if (cvlen != null)
        {
            curveLength = cvlen;
        }
        else
        {
            curveLength.CalculateLength(this);
        }
    }

    /// 计算曲线上指定位置的速度
    private float GetSplineSpeed(float t, int index)
    {
        int length = m_controlNodesPosition.Length;
        if (index != m_last_index)
        {
            p0 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, -1)];
            p1 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, 0)];
            p2 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, 1)];
            p3 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, 2)];
            m_last_index = index;
        }
        float t2 = t * t;
        Vector3 v = t2 * 0.5f * p3 + p0 * (-t2 * 0.5f + t - 0.5f) + p1 * (1.5f * t2 - 2 * t) + p2 * (-1.5f * t2 + t + 0.5f);
        return v.magnitude;
    }

    public float getSegmentLengthIn(int firstNodeIndex,
                                   float beginValue,
                                   float endValue,
                                   float step)
    {
        float len = GetSplineSpeed(beginValue, firstNodeIndex);
        len += 4f * GetSplineSpeed((beginValue + endValue) * 0.5f, firstNodeIndex);
        len += GetSplineSpeed(beginValue + endValue, firstNodeIndex);
        len *= (endValue - beginValue) / 6f;
        return len;
    }

    public Quaternion getOrientatoinOnCurve(float param, bool negativeNormal = false)
    {
        if (m_controlNodesPosition == null || m_controlNodesPosition.Length <= 0)
        {
            return Quaternion.identity;
        }
        int index;
        float newParam = param;
        RecalculateParam(param, out index, out newParam);

        Vector3 tangent = getTangentIn(newParam, index);
        Vector3 normal = getNormalIn(newParam, index);

        if (tangent.sqrMagnitude == 0f || normal.sqrMagnitude == 0f)
        {
            return Quaternion.identity;
        }
        return Quaternion.LookRotation(tangent, (negativeNormal == true ? -normal : normal));
    }

    public Vector3 GetTangentInternal(float param)
    {
        if (m_controlNodesPosition == null || m_controlNodesPosition.Length <= 0)
        {
            return Vector3.zero;
        }
        int index;
        float newParam = param;
        RecalculateParam(param, out index, out newParam);
        return getTangentIn(newParam, index);
    }

    public Vector3 GetPositionOnCurve(float param)
    {
        if (m_controlNodesPosition == null || m_controlNodesPosition.Length <= 0)
        {
            return Vector3.zero;
        }
        int index;
        float newParam;
        RecalculateParam(param, out index, out newParam);

        Vector3 ret = getPositionIn(newParam, index);
        return ret;
    }

    public void PosAndRotOnCurve(float param, out Vector3 pos, out Quaternion rot, bool negativeNormal = false)
    {
        if (m_controlNodesPosition == null || m_controlNodesPosition.Length <= 0)
        {
            pos = Vector3.zero;
            rot = Quaternion.identity;
        }
        int index;
        float newParam;
        RecalculateParam(param, out index, out newParam);
        Vector3 ret = getPositionIn(newParam, index);
        pos = ret;// myTrans.TransformPoint(ret);
        Vector3 tangent = getTangentIn(newParam, index);
        Vector3 normal = getNormalIn(newParam, index);
        if (tangent.sqrMagnitude == 0f)// || normal.sqrMagnitude == 0f)
        {
            rot = Quaternion.identity;
            return;
        }
        rot = Quaternion.LookRotation(tangent, (negativeNormal == true ? -normal : normal));
    }

    static private int GetNodeIndex(int index,
                           int arrayLength,
                           bool autoClose,
                           int offset)
    {
        int tmpIndex = index + offset;
        if (autoClose)
        {
            return (tmpIndex % arrayLength + arrayLength) % arrayLength;
        }
        else
        {
            return Mathf.Clamp(tmpIndex, 0, arrayLength - 1);
        }
    }

    private Vector3 getPositionIn(float param, int index)
    {
        /*
        return m_interpolator.InterpolateControlNode(param,
                                                     index,
                                                     m_autoClose,
                                                     m_controlNodes,
                                                   0);
         /*/
        int length = m_controlNodesPosition.Length;

        Vector3 pos = Vector3.zero;
        switch (m_interpolationMode)
        {
            case InterpolationMode.BSpline:
                {
                    if (index != m_last_index)
                    {
                        p0 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, -1)];
                        p1 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, 0)];
                        p2 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, 1)];
                        p3 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, 2)];
                        m_last_index = index;
                    }
                    /*
                    float b0, b1, b2, b3;
                    float t = param;
                    float t2 = t * t;
                    float t3 = t * t * t;

                    b0 = -0.16667f * t3 + 0.5f * t2 - 0.5f * t + 0.16667f;
                    b1 =  0.5f * t3 - t2 + 0.66667f;
                    b2 = -0.5f * t3 + 0.5f * t2 + 0.5f * t + 0.16667f;
                    b3 = t3 * 0.16667f;
                     * */
                    float b0, b1, b2, b3;
                    float ht = param * 0.5f;
                    float ht2 = param * ht;
                    float ht3 = ht2 * param;
                    b3 = 0.33333f * ht3;
                    b0 = -b3 + ht2 - ht + 0.16667f;
                    b1 = ht3 - 2 * ht2 + 0.66667f;
                    b2 = -ht3 + ht2 + ht + 0.16667f;
                    pos = b0 * p0 + b1 * p1 + b2 * p2 + b3 * p3;
                    //*/
                }
                break;
            case InterpolationMode.Hermite:
                {
                    if (index != m_last_index)
                    {
                        //{ 0, 1, -1, 2 };
                        p0 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, 0)];
                        p1 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, 1)];
                        p2 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, -1)];
                        p3 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, 2)];

                        RecalcVectors(p0, p1, ref p2, ref p3);

                        m_last_index = index;
                    }

                    float b0, b1, b2, b3;
                    float t = param;
                    float t2 = param * t;
                    float t3 = t2 * param;

                    //b0 = (float)(coefficientMatrix[0] * t3 + coefficientMatrix[1] * t2 + coefficientMatrix[2] * t + coefficientMatrix[3]);
                    //b1 = (float)(coefficientMatrix[4] * t3 + coefficientMatrix[5] * t2 + coefficientMatrix[6] * t + coefficientMatrix[7]);
                    //b2 = (float)(coefficientMatrix[8] * t3 + coefficientMatrix[9] * t2 + coefficientMatrix[10] * t + coefficientMatrix[11]);
                    //b3 = (float)(coefficientMatrix[12] * t3 + coefficientMatrix[13] * t2 + coefficientMatrix[14] * t + coefficientMatrix[15]);

                    b0 = (float)(2.0f * t3 - 3.0f * t2 + 1f);
                    b1 = (float)(-2.0f * t3 + 3.0f * t2);
                    b2 = (float)(t3 - 2.0f * t2 + t);
                    b3 = (float)(t3 - t2);

                    pos = b0 * p0 + b1 * p1 + b2 * p2 + b3 * p3;
                }
                break;
        }

        return pos;
    }

    //private double[] coefficientMatrix = new double[] {
    //         2.0, -3.0,  0.0,  1.0,
    //        -2.0,  3.0,  0.0,  0.0,
    //         1.0, -2.0,  1.0,  0.0,
    //         1.0, -1.0,  0.0,  0.0
    //    };

    public void RecalcVectors(Vector3 P0, Vector3 P1, ref Vector3 P2, ref Vector3 P3)
    {
        float tension0;
        float tension1;

        tension0 = 0.5f;
        tension1 = 0.5f;

        P2 = P1 - P2;
        P3 = P3 - P0;

        P2 = P2 * tension0;
        P3 = P3 * tension1;
    }

    private Vector3 getTangentIn(float param, int index)
    {
        int length = m_controlNodesPosition.Length;

        if (index != m_last_index)
        {
            p0 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, -1)];
            p1 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, 0)];
            p2 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, 1)];
            p3 = m_controlNodesPosition[GetNodeIndex(index, length, m_autoClose, 2)];
            m_last_index = index;
        }

        float b0, b1, b2, b3;
        float t = param;
        float t2 = t * t;

        b0 = -0.16667f * t2 + 0.5f * t - 0.5f;
        b1 = 0.5f * t2 - t;
        b2 = -0.5f * t2 + 0.5f * t + 0.5f;
        b3 = t2 * 0.16667f;

        return b0 * p0 + b1 * p1 + b2 * p2 + b3 * p3;
    }

    private Vector3 getNormalIn(float param, int index)
    {
      
        return Vector3.up;
    }

    private void RecalculateParam(float param,
                                  out int index,
                                  out float newParam)
    {
        if (param <= 0.0f)
        {
            index = 0;
            newParam = 0f;
            return;
        }
        if (param >= 1f)
        {
            index = controlNodeCount - 2;
            newParam = 1.0f;
            return;
        }

        if (curveLength.subSegLength == null)
        {
            Debug.LogError("Curve.RecalculateParam  Should not call curveLength.CalculateLength(this)!");
            curveLength.CalculateLength(this);
        }

        float[] subSegPos = curveLength.subSegPos;
        int low = 0;
        int high = subSegPos.Length - 2;
        while (low <= high)
        {
            int mid = low + ((high - low) >> 1);
            if (subSegPos[mid + 1] <= param)
                low = mid + 1;
            else if (subSegPos[mid] > param)
                high = mid - 1;
            else
            {
                int i = mid;
                int floorIndex = (i - (i % interpolateAccracy));
                index = floorIndex / interpolateAccracy;
                if (index >= controlNodeCount - 1)
                {
                    index = controlNodeCount - 2;
                    newParam = 1.0f;
                }
                else
                {
                    float invertedAccracy = 1.0f / interpolateAccracy;
                    newParam = invertedAccracy * (i - floorIndex + (param - curveLength.subSegPos[i]) / curveLength.subSegLength[i]);
                }
                return;
            }
        }

        index = controlNodeCount - 2;
        newParam = 1.0f;
    }
}

/**
 * Class CurveLength is to calculate the position of each control node,
 * and the length of each node.
 **/
public class CurveLength
{
    public void CalculateLength(Curve curve)
    {
        int subSegCount = curve.segmentCount * curve.interpolateAccracy;
        float invertedAccracy = 1.0f / curve.interpolateAccracy;

        subSegLength = new float[subSegCount];
        subSegPos = new float[subSegCount + 1];

        lengthOfCurve = 0.0f;

        for (int i = 0; i < subSegCount; ++i)
        {
            subSegLength[i] = 0.0f;
            subSegPos[i] = 0.0f;
        }

        for (int i = 0; i < curve.segmentCount; ++i)
        {
            for (int j = 0; j < curve.interpolateAccracy; ++j)
            {
                int index = i * curve.interpolateAccracy + j;
                subSegLength[index] = curve.getSegmentLengthIn(
                  i, j * invertedAccracy, (j + 1) * invertedAccracy, 0.2f * invertedAccracy);
                lengthOfCurve += subSegLength[index];
            }
        }

        float invLengthOfCurve = 1 / lengthOfCurve;
        for (int i = 0; i < subSegCount; ++i)
        {
            subSegLength[i] *= invLengthOfCurve;
            subSegPos[i + 1] = subSegPos[i] + subSegLength[i];
        }

        //Set up curve positions
        buildNodePosInCurve(curve);
    }

    private void buildNodePosInCurve(Curve curve)
    {
        float[] node_length = new float[curve.m_controlNodesPosition.Length];
        nodePosInCurve = new float[curve.m_controlNodesPosition.Length];

        for (int i = 0, count = curve.m_controlNodesPosition.Length - 1; i < count; ++i)
        {
            node_length[i] = 0.0f;
            nodePosInCurve[i] = 0.0f;
        }

        for (int i = 0, count = subSegLength.Length; i < count; ++i)
        {
            int nodeIndex = (i - i % curve.interpolateAccracy) / curve.interpolateAccracy;
            node_length[nodeIndex] += subSegLength[i];
        }

        for (int i = 0, count = curve.m_controlNodesPosition.Length - 1; i < count; ++i)
        {
            nodePosInCurve[i + 1] = nodePosInCurve[i] + node_length[i];
        }

        if (!curve.autoClose)
        {
            nodePosInCurve[curve.m_controlNodesPosition.Length - 1] = 1.0f;
        }
    }

    public float[] subSegLength;

    public float[] subSegPos;

    public float[] nodePosInCurve;

    public float lengthOfCurve; //曲线长度
}

