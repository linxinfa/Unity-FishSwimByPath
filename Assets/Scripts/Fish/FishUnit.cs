using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishUnit 
{
    public string uuid { get; private set; }
    public GameObject obj { get; private set; }
    public PathMoveByCurve moveCtrler { get; private set; }

    public FishUnit(string uuid, GameObject obj, PathMoveByCurve moveCtrler)
    {
        this.uuid = uuid;
        this.obj = obj;
        this.moveCtrler = moveCtrler;
    }

    public void Destroy()
    {
        Object.Destroy(obj);
        obj = null;
        uuid = null;
        moveCtrler = null;
    }

    public void Update()
    {
        if (null != moveCtrler)
            moveCtrler.Update();
    }
}
