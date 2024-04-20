using EditorExtend;
using Services;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[CustomEditor(typeof(RenderManagerSettings))]
public class RenderManagerEditor : AutoEditor
{
    [AutoProperty]
    public SerializedProperty asset, sceneNameList, renderDataIndexList;
    private string[] renderDataNames;
    private readonly int scneNameWidth = 100;

    protected override void OnEnable()
    {
        base.OnEnable();

        string path = FileTool.CombinePath(Application.dataPath, "Scenes");
        string[] results = Directory.GetFiles(path, "*.unity");
        int l, r;
        for (int i = 0; i < results.Length; i++)
        {
            l = results[i].LastIndexOf('\\');
            r = results[i].LastIndexOf('.');
            results[i] = results[i].Substring(l + 1, r - l - 1);
        }
        Fix(results);

        FieldInfo info = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
        ScriptableRendererData[] list = info.GetValue(asset.objectReferenceValue) as ScriptableRendererData[];
        renderDataNames = new string[list.Length];
        for (int i = 0; i < list.Length; i++)
        {
            renderDataNames[i] = list[i].name;
        }
    }

    protected override void MyOnInspectorGUI()
    {
        Rect rect, left, right;
        void UpdateRect()
        {
            rect = EditorGUILayout.GetControlRect();
            left = new Rect(rect.min, new Vector2(scneNameWidth, rect.height));
            right = new Rect(rect.min + new Vector2(scneNameWidth, 0f), new Vector2(rect.width - scneNameWidth, rect.height));
        }

        asset.PropertyField("URPAsset");
        UpdateRect();
        EditorGUI.LabelField(left, "场景名");
        EditorGUI.LabelField(right, "RenderData");
        for (int i = 0; i < sceneNameList.arraySize; i++)
        {
            UpdateRect();
            EditorGUI.LabelField(left, sceneNameList.GetArrayElementAtIndex(i).stringValue);
            renderDataIndexList.GetArrayElementAtIndex(i).intValue =
                EditorGUI.Popup(right, renderDataIndexList.GetArrayElementAtIndex(i).intValue, renderDataNames);
        }
    }

    private void Fix(string[] sceneNames)
    {
        serializedObject.Update();
        Dictionary<string, int> prev = new Dictionary<string, int>();
        for (int i = 0; i < sceneNameList.arraySize; i++)
        {
            string temp = sceneNameList.GetArrayElementAtIndex(i).stringValue;
            if (!prev.ContainsKey(temp))
                prev.Add(temp, renderDataIndexList.GetArrayElementAtIndex(i).intValue);
        }

        int length = sceneNames.Length;
        sceneNameList.ClearArray();
        for (int i = 0; i < length; i++)
        {
            sceneNameList.InsertArrayElementAtIndex(i);
            sceneNameList.GetArrayElementAtIndex(i).stringValue = sceneNames[i];
        }

        int current = renderDataIndexList.arraySize;
        if (length > current)
        {
            for (int i = current; i < length; i++)
            {
                renderDataIndexList.InsertArrayElementAtIndex(i);
            }
        }
        else
        {
            for (int i = current - 1; i > length - 1; i--)
            {
                renderDataIndexList.DeleteArrayElementAtIndex(i);
            }
        }

        for (int i = 0; i < length; i++)
        {
            string temp = sceneNameList.GetArrayElementAtIndex(i).stringValue;
            if (prev.ContainsKey(temp))
                renderDataIndexList.GetArrayElementAtIndex(i).intValue = prev[temp];
            else
                renderDataIndexList.GetArrayElementAtIndex(i).intValue = 0;
        }
        serializedObject.ApplyModifiedProperties();
    }
}