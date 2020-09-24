using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class SplinePrefab2Bin
{
    //路径预设的保存目录
    const string c_pathPrefabsSaveDir = "Assets/RawAssets/PathPrefabs";
    //路径二进制文件保存目录
    const string c_pathBinSaveDir = "Assets/GameRes/PathBin";

    public static void SavePathPrefab(Spline spline)
    {
        var fname = spline.gameObject.name;
        var targetSaveDir = string.Format("{0}/{1}.prefab", c_pathPrefabsSaveDir, fname);

#if UNITY_5
        PrefabUtility.CreatePrefab(targetSaveDir, spline.gameObject, ReplacePrefabOptions.ConnectToPrefab);
#else
        PrefabUtility.SaveAsPrefabAssetAndConnect(spline.gameObject, targetSaveDir, InteractionMode.AutomatedAction);
#endif
        AssetDatabase.SaveAssets();
    }

    public static void SaveAllPrefabs2Bin()
    {
        //获取所有的路径预设文件
        var files = Directory.GetFiles(c_pathPrefabsSaveDir, "*.prefab", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            //加载预设
            var fileAssetPath = file.Replace(Application.dataPath, "Assets");
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(fileAssetPath);

            var spline = go.GetComponent<Spline>();
            //二进制文件名，以.bytes为后缀
            var fileName = Path.GetFileName(file).Replace(".prefab", ".bytes");
            FileStream fs = new FileStream(c_pathBinSaveDir + "/" + fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //已二进制流方式写入
            BinaryWriter bw = new BinaryWriter(fs);
            //写入文件签名
            bw.Write((uint)0x2350818);
            //写入spline数据
            bw.Write((short)spline.interpolationMode);
            //写入控制点数量
            bw.Write(spline.splineNodesArray.Count);
            //遍历写入每个控制点的坐标
            foreach(var node in spline.splineNodesArray)
            {
                var pos = node.Position;
                bw.Write(pos.x);
                bw.Write(pos.y);
                bw.Write(pos.z);
            }
            bw.Close();
            fs.Close();
        }
        Debug.Log("SaveAllPrefabs2Bin Done: " + c_pathBinSaveDir);
    }
}
