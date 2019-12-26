using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[CanEditMultipleObjects]
[CustomEditor(typeof(DefaultAsset))]
public class FindAssetsEditor : Editor
{
    private string filter;
    private string[] searchInFolders;

    private string[] assets;
    void Awake()
    {
        var folders = new List<string>();

        for (int i = 0; i < targets.Length; i++)
        {
            string assetPath = AssetDatabase.GetAssetPath(targets[i]);
            if (Directory.Exists(assetPath))
                folders.Add(assetPath);
        }

        searchInFolders = folders.ToArray();
    }

    void OnDestroy()
    {

    }

    public override void OnInspectorGUI()
    {
        GUI.enabled = searchInFolders.Length == targets.Length;
        if (!GUI.enabled)
            return;

        filter = EditorGUILayout.TextField(filter);

        if (GUILayout.Button("查找"))
        {
            assets = AssetDatabase.FindAssets(filter, searchInFolders);
        }

        for (int i = 0; assets != null && i < assets.Length; i++)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(assets[i]);
            if (GUILayout.Button(new GUIContent(assetPath, AssetDatabase.GetCachedIcon(assetPath)), "Label"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                Selection.activeObject = asset;
            }
        }
    }
}