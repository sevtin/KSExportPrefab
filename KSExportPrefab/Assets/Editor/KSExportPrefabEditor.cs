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
        string exportPath = @"I:\GitHub\shaderGame";
#if UNITY_STANDALONE_WIN
        exportPath = exportPath.Replace(@"\", "/");
#endif
        exportPath = exportPath + "/";

        GameObject target = Selection.activeTransform.gameObject;

        //记录导出的资源
        Dictionary<string, Dictionary<string, string>> exportAssets = new Dictionary<string, Dictionary<string, string>>();

        InsetDictionary(exportAssets, KSAssetsType.Prefab, target.name);

        Transform[] transforms = target.GetComponentsInChildren<Transform>();
        Type monoType = new MonoBehaviour().GetType();

        foreach (var child in transforms)
        {
            foreach (var component in child.GetComponents<Component>())
            {
                Type type = component.GetType();
                string componentType = type.ToString();
                if (componentType.StartsWith(KSComponentType.UnityEngine) == false)
                {
                    InsetDictionary(exportAssets, KSAssetsType.Script, componentType);

                    while (type != monoType)
                    {
                        type = type.BaseType;
                        if (type == monoType)
                        {
                            break;
                        }
                        InsetDictionary(exportAssets, KSAssetsType.Super, type.ToString());
                    }
                }
                else if(type.Name == KSComponentType.Image)
                {
                    Image image = component as Image;
                    if (image.sprite != null)
                    {
                        InsetDictionary(exportAssets, KSAssetsType.Image, image.sprite.name);
                    }
                }
                else if (type.Name == KSComponentType.ParticleSystemRenderer)
                {
                    ParticleSystemRenderer systemRenderer = component as ParticleSystemRenderer;
                    Material material = systemRenderer.sharedMaterial;
                    if (material != null)
                    {
                        string materialName = material.name;
                        if (materialName.EndsWith(KSComponentType.Instance))
                        {
                            materialName = materialName.Replace(KSComponentType.Instance, "");
                        }
                        InsetDictionary(exportAssets, KSAssetsType.Material, materialName);
                        if (material.shader != null)
                        {
                            string shaderName = material.shader.name;
                            shaderName = shaderName.Replace("/", "-");
                            InsetDictionary(exportAssets, KSAssetsType.Shader, shaderName);
                        }
                        if (material.mainTexture != null)
                        {
                            InsetDictionary(exportAssets, KSAssetsType.Image, material.mainTexture.name);
                        }
                    }
                }
            }
        }

        //资源路劲
        Dictionary<string, Dictionary<string, string>> assetsPaths = new Dictionary<string, Dictionary<string, string>>();
        foreach (string type in exportAssets.Keys)
        {
            assetsPaths.Add(type, GetAssetPaths(exportAssets[type], KSAssetsType.GetSuffixName(type)));
        }

        foreach (string type in assetsPaths.Keys)
        {
            //导出资源
            ExportAssets(assetsPaths[type], exportPath);
            NoteExportList(type, assetsPaths[type]);
        }
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

    static void InsetDictionary(Dictionary<string, Dictionary<string, string>> dict, string type, string value)
    {
        if(value == string.Empty)
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
        if (assetPath.EndsWith("/" + filter + "." + type))
        {
            return assetPath;
        }
        return string.Empty;
    }

    static void ExportAssets(Dictionary<string, string> sourcePaths, string exportPath)
    {
        bool overwrite = true;
        foreach (string path in sourcePaths.Keys)
        {
            ExportAssets(path, exportPath, overwrite);
        }
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

    static void NoteExportList(string filename, Dictionary<string, string> assets)
    {
        string notePath = Application.dataPath + "/Resources/ExportNotes/";

        if (!Directory.Exists(notePath))
        {
            Directory.CreateDirectory(notePath);
        }
        notePath = notePath + filename + ".txt";

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
                return "png";
            case Prefab:
                return "prefab";
            case Script:
            case Super:
                return "cs";
            case Material:
                return "mat";
            case Shader:
                return "shader";
        }
        return string.Empty;
    }
}

public static class KSComponentType
{
    public const String UnityEngine = "UnityEngine";
    public const string Image = "Image";
    public const String ParticleSystemRenderer = "ParticleSystemRenderer";
    public const String Instance = " (Instance)";
}