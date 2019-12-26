using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class UIBackgroundPanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        test1();
        return;

        Object[] componments = GameObject.FindObjectsOfTypeAll(typeof(Image));
        ArrayList images = new ArrayList();
        for (int i = 0; i < componments.Length; i++)
        {
            string name = (componments[i] as Image).sprite.name;
            images.Add(name);
            Debug.Log("组件的名字:" + name);
        }
        writeFile("image",images);
        return;

    
        GameObject[] pAllObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));

        foreach (GameObject pObject in pAllObjects)
        {
            if (pObject.transform.parent != null)
            {
                continue;
            }

            if (pObject.hideFlags == HideFlags.NotEditable || pObject.hideFlags == HideFlags.HideAndDontSave)
            {
                continue;
            }

            // if (Application.isEditor)
            // {
            // string sAssetPath = AssetDatabase.GetAssetPath(pObject.transform.root.gameObject);
            // if (!string.IsNullOrEmpty(sAssetPath))
            // {
            // continue;
            // }
            // }

            Debug.Log(pObject.name);
        }
    }
    void test1(){

        ArrayList images = new ArrayList();
        ArrayList scripts = new ArrayList();
        Transform[] transforms = GetComponentsInChildren<Transform>();
        foreach (var child in transforms)
        {
            
            Image image = child.gameObject.GetComponent<Image>();
            if(image != null){
                images.Add(image.sprite.name);
            }
            foreach (var component in child.GetComponents<Component>())
            {
                string componentType = component.GetType().ToString();
                if(componentType.StartsWith("UnityEngine") == false){
                    scripts.Add(componentType + ".cs");
                }
            }
        }
        writeFile("image",images);
        writeFile("script",scripts);
    }
    void test3(){
        //string assetPath = AssetDatabase.GetAssetPath;
    }


    void writeFile(string name,ArrayList texts){
        string path = Application.dataPath + "/Resources/Text/" + name + ".txt";
        string context = string.Empty;
        for(int i = 0;i < texts.Count;i++){
            context = context + texts[i] + "\r\n";
        }

        // 文件流创建一个文本文件
        FileStream file = new FileStream(path, FileMode.Create);
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
