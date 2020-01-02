using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System;

public class KSExportPrefabEditor
{

    [MenuItem("KSMenu/Export Prefab")]
    static void ExportPrefab()
    {
        string exportPath = @"I:\GitHub\ShareGame";
#if UNITY_STANDALONE_WIN
        exportPath = exportPath.Replace(@"\", "/");
#endif
        exportPath = exportPath + "/";

        //记录导出的资源
        Dictionary<string, Dictionary<string, string>> exportAssets = new Dictionary<string, Dictionary<string, string>>();

        //1、Prefab
        GameObject target = Selection.activeTransform.gameObject;
        InsetDictionary(exportAssets, KSAssetsType.Prefab, GetAssetPath(target.name, KSAssetsType.Prefab));

        Transform[] transforms = target.GetComponentsInChildren<Transform>();
        Type monoType = new MonoBehaviour().GetType();

        foreach (var child in transforms)
        {
            foreach (var component in child.GetComponents<Component>())
            {
                Type type = component.GetType();
                string componentName = type.ToString();
                if (componentName.StartsWith(KSComponentType.UnityEngine) == false)
                {//2、Script
                    InsetDictionary(exportAssets, KSAssetsType.Script, GetAssetPath(componentName, KSAssetsType.Script));
                    while (type != monoType)
                    {
                        type = type.BaseType;
                        if (type == monoType)
                        {
                            break;
                        }
                        InsetDictionary(exportAssets, KSAssetsType.Super, GetAssetPath(type.ToString(), KSAssetsType.Script));
                    }
                }
                else if (type.Name == KSComponentType.Image)
                {//3、Image
                    Image image = component as Image;
                    if(image.sprite != null)
                    {
                        NotesAssetsPath(exportAssets, KSAssetsType.Image, image.sprite);
                    }
                    
                }
                else if (type.Name == KSComponentType.SpriteRenderer)
                {//4、SpriteRenderer
                    SpriteRenderer spriteRenderer = component as SpriteRenderer;
                    if(spriteRenderer.sprite != null)
                    {
                        NotesAssetsPath(exportAssets, KSAssetsType.Image, spriteRenderer.sprite);
                    }
                }
                else if (type.Name == KSComponentType.ParticleSystemRenderer)
                {
                    ParticleSystemRenderer systemRenderer = component as ParticleSystemRenderer;
                    Material material = systemRenderer.sharedMaterial;
                    if (material != null)
                    {//5、Material
                        NotesAssetsPath(exportAssets, KSAssetsType.Material, material);
                        if (material.shader != null)
                        {//6、Shader
                            NotesAssetsPath(exportAssets, KSAssetsType.Shader, material.shader);
                        }
                        if (material.mainTexture != null)
                        {//7、Image
                            NotesAssetsPath(exportAssets, KSAssetsType.Image, material.mainTexture);
                        }
                    }
                }
            }
        }

        foreach (string type in exportAssets.Keys)
        {
            //导出资源
            ExportAssets(exportAssets[type], exportPath);
            NoteExportList(type, exportAssets[type]);
        }
        Debug.Log("执行完毕");
    }

    static void NotesAssetsPath(Dictionary<string, Dictionary<string, string>> dict, string type, UnityEngine.Object obj)
    {
        string assetPath = AssetDatabase.GetAssetPath(obj);
        if (assetPath.EndsWith(KSAssetsType.GetSuffixName(type)))
        {
            InsetDictionary(dict, type, assetPath);
        }
    }

    static void InsetDictionary(Dictionary<string, Dictionary<string, string>> dict, string type, string value)
    {
        if (value == string.Empty)
        {
            return;
        }
        if (dict.ContainsKey(type) == false)
        {
            dict.Add(type, new Dictionary<string, string>());
            dict[type].Add(value, value);
        }
        else
        {
            if (dict[type].ContainsKey(value) == false)
            {
                dict[type].Add(value, value);
            }
        }
    }
    /*
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
                    if (paths.ContainsKey(assetPath) == false)
                    {
                        paths.Add(assetPath, assetPath);
                    }
                }
            }
        }
        return paths;
    }
    
    static string GetAssetPath(string guid, string filter, string type)
    {
        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
        if (assetPath.EndsWith("/" + filter + type))
        {
            return assetPath;
        }
        return string.Empty;
    }
    */
    static string GetAssetPath(string name, string type)
    {
        string assetPath = string.Empty;
        string[] resules = AssetDatabase.FindAssets(name);
        for (int i = 0; i < resules.Length; i++)
        {
            assetPath = AssetDatabase.GUIDToAssetPath(resules[i]);
            if (assetPath != string.Empty)
            {
                if (assetPath.EndsWith("/" + name + KSAssetsType.GetSuffixName(type)))
                {
                    return assetPath;
                }

            }
        }
        return assetPath;
    }

    static void ExportAssets(Dictionary<string, string> sourcePaths, string exportPath)
    {
        bool overwrite = true;
        foreach (string path in sourcePaths.Keys)
        {
            ExportAssets(path, exportPath, overwrite);
        }
    }

    static string GetAssetName(string path)
    {
        string[] strArray = path.Split('/');
        if (strArray.Length == 0)
        {
            return string.Empty;
        }
        return strArray[strArray.Length - 1];
    }

    static void ExportAssets(string sourcePath, string exportPath, bool overwrite)
    {
        string fileName = GetAssetName(sourcePath);
        if (fileName == string.Empty)
        {
            return;
        }
        exportPath = exportPath + sourcePath.Replace(fileName, "");
        sourcePath = Application.dataPath + sourcePath.Replace(KSPath.Assets, "");
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }
        File.Copy(sourcePath, Path.Combine(exportPath, Path.GetFileName(fileName)), overwrite);
        File.Copy(sourcePath + KSSuffix.meta, Path.Combine(exportPath, Path.GetFileName(fileName + KSSuffix.meta)), overwrite);
    }

    static void NoteExportList(string filename, Dictionary<string, string> assets)
    {
        string notePath = Application.dataPath + KSPath.ExportNotes;

        if (!Directory.Exists(notePath))
        {
            Directory.CreateDirectory(notePath);
        }
        notePath = notePath + filename + KSSuffix.txt;

        string context = string.Empty;
        foreach (string key in assets.Keys)
        {
            context = context + key + "\r\n";
        }

        // 文件流创建一个文本文件
        FileStream file = new FileStream(notePath, FileMode.Create);
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

public static class KSPath {
    public const string Assets = "Assets";
    public const string ExportNotes = "/Resources/ExportNotes/";
}

public static class KSAssetsType
{
    public const string Image = "KSImage";
    public const string Prefab = "KSPrefab";
    public const string Script = "KSScript";
    public const string Super = "KSSuper";
    public const string Shader = "KSShader";
    public const string Material = "KSMaterial";
    
    public static string GetSuffixName(string type)
    {
        switch (type)
        {
            case Image:
                return KSSuffix.png;
            case Prefab:
                return KSSuffix.prefab;
            case Script:
            case Super:
                return KSSuffix.cs;
            case Material:
                return KSSuffix.mat;
            case Shader:
                return KSSuffix.shader;
        }
        return string.Empty;
    }
}

public static class KSSuffix
{
    public const string png = ".png";
    public const string jpg = ".jpg";
    public const string prefab = ".prefab";
    public const string cs = ".cs";
    public const string mat = ".mat";
    public const string shader = ".shader";
    public const string meta = ".meta";
    public const string txt = ".txt";
}

public static class KSComponentType
{
    public const string UnityEngine = "UnityEngine";
    public const string Image = "Image";
    public const string SpriteRenderer = "SpriteRenderer";
    public const string ParticleSystemRenderer = "ParticleSystemRenderer";
    public const string Instance = " (Instance)";
}