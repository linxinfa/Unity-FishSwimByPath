using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class SplinePrefab2Bin
{
    //路径预设的保存目录
    const string c_pathPrefabsSaveDir = "Assets/RawAssets/PathPrefabs";
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
        var files = Directory.GetFiles(c_pathPrefabsSaveDir, "*.prefab", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var fileAssetPath = file.Replace(Application.dataPath, "Assets");
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(fileAssetPath);
            var spline = go.GetComponent<Spline>();
            var fileName = Path.GetFileName(file).Replace(".prefab", ".bytes");
            FileStream fs = new FileStream(c_pathBinSaveDir + "/" + fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter bw = new BinaryWriter(fs);
            //写入文件签名
            bw.Write((uint)0x2350818);
            //写入spline数据
            bw.Write((short)spline.interpolationMode);

            bw.Write(spline.splineNodesArray.Count);
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
