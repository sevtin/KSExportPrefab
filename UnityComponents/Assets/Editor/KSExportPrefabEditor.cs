using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class KSExportPrefabEditor
{
    [MenuItem("KSMenu/Export Prefab")]
    static void ExportPrefab()
    {
        GameObject target = Selection.activeTransform.gameObject;

        ArrayList prefabs = new ArrayList();
        ArrayList images = new ArrayList();
        ArrayList scripts = new ArrayList();

        prefabs.Add(target.name);

        Transform[] transforms = target.GetComponentsInChildren<Transform>();
        foreach (var child in transforms)
        {
            Image image = child.gameObject.GetComponent<Image>();
            if (image != null)
            {
                images.Add(image.sprite.name);
            }
            foreach (var component in child.GetComponents<Component>())
            {
                string componentType = component.GetType().ToString();
                if (componentType.StartsWith("UnityEngine") == false)
                {
                    scripts.Add(componentType);
                }
            }
        }

        ArrayList prefabsPath = GetAssetPaths(prefabs, "prefab");
        ArrayList imagesPath = GetAssetPaths(images, "png");
        ArrayList scriptsPath = GetAssetPaths(scripts, "cs");

        string exportPath = @"/Volumes/data/UnityApp/NewCopyProject/";
#if UNITY_STANDALONE_WIN
        exportPath = exportPath.Replace(@"\", "/");
#endif

        bool overwrite = true;
        foreach (string path in prefabsPath)
        {
            ExportAssets(path, exportPath, overwrite);
        }
        foreach (string path in imagesPath)
        {
            ExportAssets(path, exportPath, overwrite);
        }
        foreach (string path in scriptsPath)
        {
            ExportAssets(path, exportPath, overwrite);
        }
        Debug.Log("执行完毕");
    }

    static ArrayList GetAssetPaths(ArrayList assets, string type)
    {
        ArrayList paths = new ArrayList();
        foreach (string filter in assets)
        {
            string[] resules = AssetDatabase.FindAssets(filter);
            for (int i = 0; i < resules.Length; i++)
            {
                var assetPath = GetAssetPath(resules[i], filter, type);
                if (assetPath != string.Empty)
                {
                    paths.Add(assetPath);
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
}