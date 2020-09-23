using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PathMgr
{
    public Curve GetPathSplineByBin(string binName)
    {
        Curve curve = null;
#if UNITY_EDITOR
        //编辑器环境下，直接从磁盘中读取文件
        var binFullDir = Application.dataPath + "/GameRes/PathBin/" + binName;
        if (File.Exists(binFullDir))
        {
            FileStream fs = new FileStream(binFullDir, FileMode.Open, FileAccess.Read);
            BinaryReader rd = new BinaryReader(fs);
            curve = ParseBin2Curve(rd);
            fs.Close();
            rd.Close();
        }
#else

#endif


        return curve;
    }

    private Curve ParseBin2Curve(BinaryReader rd)
    {
        var curve = new Curve();
        //读取逻辑要和SplinePrefab2Bin的写入逻辑匹配
        var sig = rd.ReadInt32();
        //校验签名
        Debug.Assert(0x2350818 == sig, "Path sig Error");
        curve.interpolationMode = (Curve.InterpolationMode)rd.ReadInt16();

        List<Vector3> posList = new List<Vector3>();
        var nodeCnt = rd.ReadInt32();
        for (int i = 0; i < nodeCnt; ++i)
        {
            var pos = new Vector3(rd.ReadSingle(), rd.ReadSingle(), rd.ReadSingle());
            posList.Add(pos);
        }
        curve.SetPoints(posList.ToArray());
        return curve;
    }

    static PathMgr s_instance;
    public static PathMgr instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new PathMgr();
            return s_instance;
        }
    }
}
