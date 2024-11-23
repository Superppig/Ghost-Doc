using System;
using System.Collections.Generic;
using Services;
using UnityEngine;

public class UIManager : Service, IService
{
    public override Type RegisterType => typeof(UIManager);

    public static Dictionary<string, View> m_Views = new Dictionary<string, View>();

    protected internal override void Init()
    {
        base.Init();
        LoadPrefabs();
        Initilization();
    }

    void LoadPrefabs()
    {
        var prefabs = Resources.LoadAll<Transform>("UIPerfabs");
        foreach (var view in prefabs)
        {
            if (!m_Views.ContainsKey(view.name))
            {
                Transform prefab = Instantiate(view,transform);
                prefab.name = prefab.name.Replace("(Clone)", "");
                m_Views.Add(prefab.name, prefab.GetComponent<View>());
            }
        }
    }
    void Initilization()
    {
        foreach (var view in  m_Views)
        {
            view.Value.Init();
            view.Value.Hide();
        }
    }
    
    /// <summary>
    /// 显示视图
    /// </summary>
    /// <typeparam name="T">视图类型</typeparam>
    public void ShowView<T> () where T : View
    {
        foreach (var item in m_Views)
        {
            if (item.Value is T )
            {
                item.Value.Show();
            }
        }
    }
    
    /// <summary>
    /// 关闭视图
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void CloseView<T>()where T : View
    {
        foreach (var item in m_Views)
        {
            if (item.Value is T )
            {
                item.Value.Hide();
            }
        }
    }
    
    /// <summary>
    /// 获取视图
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetView<T>() where T : View
    {
        foreach (var item in m_Views)
        {
            if (item.Value is T tItem)
            {
                return tItem;
            }
        }
        return null;
    }
}