using Services.SceneManagement;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu]
public class RenderManagerSettings : ScriptableObject
{
    public UniversalRenderPipelineAsset asset;

    [SerializeField]
    private List<string> sceneNameList;
    [SerializeField]
    private List<int> renderDataIndexList;

    private Dictionary<int, int> searcher;
    private FieldInfo indexInfo;

    public void Initialize()
    {
        indexInfo = asset.GetType().GetField("m_DefaultRendererIndex", BindingFlags.Instance | BindingFlags.NonPublic);
        searcher = new Dictionary<int, int>();
        for (int i = 0; i < sceneNameList.Count; i++)
        {
            int index = SceneControllerUtility.ToSceneIndex(sceneNameList[i]);
            if (index != -1)
                searcher.Add(index, renderDataIndexList[i]);
        }
    }

    public void TryUpdateRenderData(int sceneIndex)
    {
        int index = 0;
        if (searcher.ContainsKey(sceneIndex))
            index = searcher[sceneIndex];
        indexInfo.SetValue(asset, index);
    }
}
