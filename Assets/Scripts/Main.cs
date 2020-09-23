using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        var curve = PathMgr.instance.GetPathSplineByBin("Path01.bytes");
        var moveObj = new GameObject("moveObj");
        var move = moveObj.AddComponent<PathMoveByCurve>();
        move.curve = curve;

        var fishObj = ResourceMgr.instance.Instantiate<GameObject>("Fish/FishModel.prefab");
        fishObj.transform.SetParent(moveObj.transform, false);
    }
}
