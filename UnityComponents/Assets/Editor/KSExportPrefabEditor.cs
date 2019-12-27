using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class KSExportPrefabEditor
{
    [MenuItem("KSMenu/Export Prefab")]
    static void ExportPrefab()
    {
        GameObject target = Selection.activeTransform.gameObject;

        Dictionary<string, string> prefabs = new Dictionary<string, string>();
        Dictionary<string, string> images = new Dictionary<string, string>();
        Dictionary<string, string> scripts = new Dictionary<string, string>();

        InsetDictionary(prefabs, target.name, target.name);

        Transform[] transforms = target.GetComponentsInChildren<Transform>();
        foreach (var child in transforms)
        {
            Image image = child.gameObject.GetComponent<Image>();
            if (image != null && image.sprite != null)
            {
                InsetDictionary(images, image.sprite.name, image.sprite.name);
            }
            foreach (var component in child.GetComponents<Component>())
            {
                string componentType = component.GetType().ToString();
                if (componentType.StartsWith("UnityEngine") == false)
                {
                    InsetDictionary(scripts, componentType, componentType);
                }
            }
        }

        Dictionary<string, string> prefabsPath = GetAssetPaths(prefabs, "prefab");
        Dictionary<string, string> imagesPath = GetAssetPaths(images, "png");
        Dictionary<string, string> scriptsPath = GetAssetPaths(scripts, "cs");

        string exportPath = @"H:\UnityProject\CopyAss";
#if UNITY_STANDALONE_WIN
        exportPath = exportPath.Replace(@"\", "/");
#endif
        exportPath = exportPath + "/";
        bool overwrite = true;
        foreach (string path in prefabsPath.Keys)
        {
            ExportAssets(path, exportPath, overwrite);
        }
        foreach (string path in imagesPath.Keys)
        {
            ExportAssets(path, exportPath, overwrite);
        }
        foreach (string path in scriptsPath.Keys)
        {
            ExportAssets(path, exportPath, overwrite);
        }

        SaveExportList("ExportPrefabs", prefabsPath);
        SaveExportList("ExportImages", imagesPath);
        SaveExportList("ExportScripts", scriptsPath);

        Debug.Log("执行完毕");
    }

    static void Deduplication(ArrayList list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                if (list[i].Equals(list[j]))
                {
                    list.RemoveAt(i);
                }
            }
        }
    }

    static void InsetDictionary(Dictionary<string, string> ditc,string key,string value)
    {
        if(ditc.ContainsKey(key) == false)
        {
            ditc.Add(key, value);
        }
    }

    static Dictionary<string, string> GetAssetPaths(Dictionary<string, string> assets, string type)
    {
        Dictionary<string, string> paths = new Dictionary<string, string>();
        foreach (string filter in assets.Keys)
        {
            string[] resules = AssetDatabase.FindAssets(filter);
            for (int i = 0; i < resules.Length; i++)
            {
                var assetPath = GetAssetPath(resules[i], filter, type);
                if (assetPath != string.Empty)
                {
                    InsetDictionary(paths, assetPath, assetPath);
                }
            }
        }
        return paths;
    }
    static string GetAssetPath(string guid, string filter, string type)
    {
        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
        if (assetPath.EndsWith("/" + filter + "." + type))
        {
            return assetPath;
        }
        return string.Empty;
    }

    static void ExportAssets(string sourcePath, string exportPath, bool overwrite)
    {
        string[] strArray = sourcePath.Split('/');
        if (strArray.Length == 0)
        {
            return;
        }

        string fileName = strArray[strArray.Length - 1];
        exportPath = exportPath + sourcePath.Replace(fileName, "");
        sourcePath = Application.dataPath + sourcePath.Replace("Assets", "");
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }
        File.Copy(sourcePath, Path.Combine(exportPath, Path.GetFileName(fileName)), overwrite);
        File.Copy(sourcePath + ".meta", Path.Combine(exportPath, Path.GetFileName(fileName + ".meta")), overwrite);
    }

    static void SaveExportList(string filename, Dictionary<string, string> assets)
    {
        string savePath = Application.dataPath + "/Resources/ExportNotes/";

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        savePath = savePath + filename + ".txt";

        string context = string.Empty;
        foreach (string key in assets.Keys)
        {
            context = context + key + "\r\n";
        }

        // 文件流创建一个文本文件
        FileStream file = new FileStream(savePath, FileMode.Create);
        //得到字符串的UTF8 数据流
        byte[] bts = System.Text.Encoding.UTF8.GetBytes(context);
        // 文件写入数据流
        file.Write(bts, 0, bts.Length);
        if (file != null)
        {
            //清空缓存
            file.Flush();
            // 关闭流
            file.Close();
            //销毁资源
            file.Dispose();
        }
    }
}