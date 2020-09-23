using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplinePathData 
{
    public SplinePathData()
    {
        splineType = (short)Spline.InterpolationMode.BSpline;
        rotationMode = (short)Spline.RotationMode.Tangent;

        autoClose = 1;
        posList = new List<Vector3>();
    }

    public short splineType;
    public short rotationMode;
    public short autoClose;
    public List<Vector3> posList;
}

