using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System;

namespace KSMenuEditor
{
    public class KSExportPrefabEditor
    {
        //记录导出的资源
        static Dictionary<string, Dictionary<string, string>> exportAssets = new Dictionary<string, Dictionary<string, string>>();

        [MenuItem("KSMenu/Export Prefab")]
        static void ExportPrefab()
        {
            string exportPath = @"H:\UnityProject\ExNew";
#if UNITY_STANDALONE_WIN
            exportPath = exportPath.Replace(@"\", "/");
#endif
            exportPath = exportPath + "/";
            exportAssets.Clear();

            //Prefab
            GameObject target = Selection.activeTransform.gameObject;
            RecordGo(target);

            string[] keys = new string[exportAssets.Count];
            exportAssets.Keys.CopyTo(keys, 0);
            foreach (string type in keys)
            {
                //导出资源
                ExportAssets(exportAssets[type], exportPath);
            }

            foreach (string type in exportAssets.Keys)
            {
                //记录原路径
                NoteExportList(type, target.name, exportAssets[type]);
            }
            NoteDirectorys(target.name);
            Debug.Log("执行完毕");
        }

        static void RecordGo(GameObject target)
        {
            if (target == null)
            {
                return;
            }
            /*
            if (type == KSObjectType.Prefab)
            {
                InsetDictionary(exportAssets, KSAssetsType.Prefab, GetAssetPath(target.name, KSAssetsType.Prefab));
            }*/

            Transform[] transforms = target.GetComponentsInChildren<Transform>();

            foreach (Transform transform in transforms)
            {
                string prefabPath = GetAssetPath(transform.name, KSAssetsType.Prefab, true);
                if (prefabPath != string.Empty)
                {
                    InsetDictionary(KSAssetsType.Prefab, prefabPath);
                }

                foreach (Component component in transform.GetComponents<Component>())
                {
                    RecordExportAsset(component);
                }

                for (int i = 0; i < transform.childCount; i++)
                {
                    GameObject child = transform.GetChild(i).gameObject;
                    foreach (Component component in child.GetComponents<Component>())
                    {
                        RecordExportAsset(component);
                    }
                }
            }
        }

        static void RecordExportAsset(Component component)
        {
            if (component == null)
            {
                return;
            }

            Type type = component.GetType();
            string typeName = type.Name;
            string componentName = type.ToString();

            if (componentName.StartsWith(KSComponentType.UnityEngine) == false)
            {//1、Script
                RecordScript(component);
            }
            else if (componentName.StartsWith(KSComponentType.UnityEngineUI))
            {//2、自定义UI
                RecordUI(component);
            }
            else if (typeName == KSComponentType.Image)
            {//3、Image
                Image image = component as Image;
                RecordImage(image);
            }
            else if (typeName == KSComponentType.RawImage)
            {//4、RawImage
                RawImage rawImage = component as RawImage;
                RecordRawImage(rawImage);
            }
            else if (typeName == KSComponentType.SpriteRenderer)
            {//5、SpriteRenderer
                SpriteRenderer spriteRenderer = component as SpriteRenderer;
                RecordSpriteRenderer(spriteRenderer);
            }
            else if (typeName == KSComponentType.ParticleSystemRenderer)
            {//6、ParticleSystemRenderer
                ParticleSystemRenderer systemRenderer = component as ParticleSystemRenderer;
                RecordMaterial(systemRenderer.sharedMaterial);
            }
            else if (typeName == KSComponentType.Animator)
            {//7、Animator
                Animator animator = component as Animator;
                RecordAnimator(animator);
            }
        }

        static Type monoType = typeof(MonoBehaviour);
        static void RecordScript(Component component)
        {
            Type type = component.GetType();
            string componentName = type.ToString();

            InsetDictionary(KSAssetsType.Script, GetAssetPath(componentName, KSAssetsType.Script));

            //1 image
            Image image = component.GetComponent<Image>();
            RecordImage(image);

            //2 RawImage
            RawImage rawImage = component.GetComponent<RawImage>();
            RecordRawImage(rawImage);

            //3 Super
            while (type != monoType)
            {
                type = type.BaseType;
                if (type == monoType)
                {
                    break;
                }
                InsetDictionary(KSAssetsType.Super, GetAssetPath(type.ToString(), KSAssetsType.Script));
            }
        }

        static void RecordUI(Component component)
        {
            Type type = component.GetType();
            string componentName = type.ToString();

            string[] strArray = componentName.Split('.');
            if (strArray.Length > 0)
            {
                InsetDictionary(KSAssetsType.Script, GetAssetPath(strArray[strArray.Length - 1], KSAssetsType.Script));
            }
            //1 image
            Image image = component.GetComponent<Image>();
            RecordImage(image);

            //2 RawImage
            RawImage rawImage = component.GetComponent<RawImage>();
            RecordRawImage(rawImage);
        }

        static void RecordSprite(Sprite sprite)
        {
            if (sprite == null)
            {
                return;
            }
            NotesAssetsPath(KSAssetsType.Image, sprite);
        }

        static void RecordTexture(Texture texture)
        {
            if (texture == null)
            {
                return;
            }
            NotesAssetsPath(KSAssetsType.Image, texture);
        }

        static void RecordImage(Image image)
        {
            if (image == null)
            {
                return;
            }
            RecordSprite(image.sprite);
            RecordMaterial(image.material);
        }

        static void RecordRawImage(RawImage rawImage)
        {
            if (rawImage == null)
            {
                return;
            }
            RecordTexture(rawImage.mainTexture);
            RecordMaterial(rawImage.material);
        }

        static void RecordSpriteRenderer(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null)
            {
                return;
            }
            RecordSprite(spriteRenderer.sprite);
            RecordMaterial(spriteRenderer.sharedMaterial);
        }

        static void RecordMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }
            if (material.name.StartsWith(KSUnwanted.MaterialDefault) == false)
            {
                NotesAssetsPath(KSAssetsType.Material, material);
            }
            if (material.shader != null)
            {//Shader
                if (material.shader.name.StartsWith(KSUnwanted.ShaderDefault) == false)
                {
                    NotesAssetsPath(KSAssetsType.Shader, material.shader);
                }
            }
            if (material.mainTexture != null)
            {//Image
                NotesAssetsPath(KSAssetsType.Image, material.mainTexture);
            }
        }

        static void RecordAnimator(Animator animator)
        {
            if (animator == null)
            {
                return;
            }
            RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;
            NotesAssetsPath(KSAssetsType.Animator, runtimeAnimatorController);

            foreach (AnimationClip clip in runtimeAnimatorController.animationClips)
            {
                /*
                foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                }
                */
                Type type = typeof(SpriteRenderer);
                foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                {
                    if (binding.type == type)
                    {
                        ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                        foreach (ObjectReferenceKeyframe keyframe in keyframes)
                        {
                            RecordSprite(keyframe.value as Sprite);
                        }
                    }
                }
                NotesAssetsPath(KSAssetsType.AnimationClip, clip);
            }
        }

        static void NotesAssetsPath(string type, UnityEngine.Object obj)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (type == KSAssetsType.Image)
            {
                if (assetPath.EndsWith(KSSuffix.png) || assetPath.EndsWith(KSSuffix.jpg))
                {
                    InsetDictionary(type, assetPath);
                }
            }
            else
            {
                if (assetPath.EndsWith(KSAssetsType.GetSuffixName(type)))
                {
                    InsetDictionary(type, assetPath);
                }
            }
        }

        static List<string> unwanted_scripts = KSUnwanted.GetUnwantedScripts();
        static List<string> unwanted_images = KSUnwanted.GetUnwantedImages();
        static void InsetDictionary(string type, string value)
        {
            if (value == string.Empty)
            {
                return;
            }
            string fileName = GetAssetName(value);
            if (unwanted_scripts.Count > 0)
            {
                if (type == KSAssetsType.Script || type == KSAssetsType.Super)
                {
                    if (unwanted_scripts.Contains(fileName))
                    {
                        return;
                    }
                }
            }
            if (unwanted_images.Count > 0)
            {
                if (type == KSAssetsType.Image)
                {
                    if (unwanted_images.Count > 0)
                    {
                        if (unwanted_images.Contains(fileName))
                        {
                            return;
                        }
                    }
                }
            }

            if (exportAssets.ContainsKey(type) == false)
            {
                exportAssets.Add(type, new Dictionary<string, string>());
                exportAssets[type].Add(value, value);
            }
            else
            {
                if (exportAssets[type].ContainsKey(value) == false)
                {
                    exportAssets[type].Add(value, value);
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
        static string GetAssetPath(string name, string type, bool isPrefab = false)
        {
            string assetPath = string.Empty;
            string[] resules = AssetDatabase.FindAssets(name);
            for (int i = 0; i < resules.Length; i++)
            {
                assetPath = AssetDatabase.GUIDToAssetPath(resules[i]);
                if (assetPath != string.Empty)
                {
                    string end = "/" + name + KSAssetsType.GetSuffixName(type);
                    if (assetPath.EndsWith(end))
                    {
                        return assetPath;
                    }
                }
            }
            if (isPrefab)
            {
                return string.Empty;
            }
            return assetPath;
        }

        static void ExportAssets(Dictionary<string, string> sourcePaths, string exportPath)
        {
            foreach (string path in sourcePaths.Keys)
            {
                ExportAssets(path, exportPath);
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

        static string GetDirectoryPath(string path)
        {
            string[] strArray = path.Split('/');
            if (strArray.Length == 0)
            {
                return string.Empty;
            }
            return path.Substring(0, path.Length - strArray[strArray.Length - 1].Length);
        }

        static bool isOverride = false;
        static void ExportAssets(string sourcePath, string exportPath)
        {
            string fileName = GetAssetName(sourcePath);
            if (fileName == string.Empty)
            {
                return;
            }
            exportPath = exportPath + sourcePath.Replace(fileName, "");
            sourcePath = Application.dataPath + sourcePath.Replace(KSPath.Assets, "");
            if (!Directory.Exists(exportPath))
            {//目录
                Directory.CreateDirectory(exportPath);
            }
            try
            {
                File.Copy(sourcePath, Path.Combine(exportPath, Path.GetFileName(fileName)), isOverride);
                File.Copy(sourcePath + KSSuffix.meta, Path.Combine(exportPath, Path.GetFileName(fileName + KSSuffix.meta)), isOverride);
            }
            catch
            {
                KSDebug.Log("写入失败: " + fileName);
                InsetDictionary(KSAssetsType.Error, sourcePath);
            }
        }

        static void NoteDirectorys(string name)
        {
            Dictionary<string, string> directory = new Dictionary<string, string>();
            foreach (string type in exportAssets.Keys)
            {
                foreach (string file in exportAssets[type].Keys)
                {
                    string path = GetDirectoryPath(file);
                    if (directory.ContainsKey(path) == false)
                    {
                        directory[path] = path;
                    }
                }
            }
            NoteExportList(KSAssetsType.Directory, name, directory);
        }

        static void NoteExportList(string filename, string assetsName, Dictionary<string, string> assets)
        {
            string notePath = Application.dataPath + KSPath.ExportNotes + assetsName + "/";

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

    public static class KSUnwanted
    {
        public const string MaterialDefault = "Sprites-Default";
        public const string ShaderDefault = "Sprites/Default";

        public static List<string> GetUnwantedScripts()
        {
            List<string> unwanteds = new List<string> { "UICustomTextFont.cs",
                "UICustomButton.cs",
                "EvonyImage.cs",
                "UIEmojiImage.cs",
                "FxImage.cs",
                "EvonyText.cs",
                "UIEmojiText.cs",
                "UIBtnTextColor.cs",
                "UIFXObj.cs" };
            return unwanteds;
        }
        public static List<string> GetUnwantedImages()
        {
            List<string> unwanteds = new List<string> { "icon_E_48.png" };
            return unwanteds;
        }
    }

    public static class KSPath
    {
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
        //public const string Animation = "KSAnimation";
        public const string Animator = "KSAnimator";
        public const string AnimationClip = "KSAnimationClip";
        public const string Error = "KSError";
        public const string Directory = "KSDirectory";

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
                case AnimationClip:
                    return KSSuffix.anim;
                case Animator:
                    return KSSuffix.controller;
                default:
                    return string.Empty;
            }
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
        public const string anim = ".anim";
        public const string controller = ".controller";
    }

    public static class KSComponentType
    {
        public const string UnityEngine = "UnityEngine";
        public const string UnityEngineUI = "UnityEngine.UI.KS";
        public const string Image = "Image";
        public const string RawImage = "RawImage";
        public const string SpriteRenderer = "SpriteRenderer";
        public const string ParticleSystemRenderer = "ParticleSystemRenderer";
        public const string Texture = "Texture";
        //public const string Animation = "Animation";
        public const string Animator = "Animator";
        public const string AnimationClip = "AnimationClip";
    }

    enum KSObjectType
    {
        Prefab,
        Obj,
    }

    public class KSDebug
    {
        public static void Log(object message)
        {
            Debug.Log("|---------| " + message + " |---------|");
        }
    }
}