using System;
using UnityEngine;
using UnityEditor;

class TestShaderGUI : ShaderGUI
{
    MaterialEditor materialEditor;
    MaterialProperty[] materialProperty;
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        this.materialEditor = materialEditor;
        materialProperty = properties;
        Show();

        base.OnGUI(materialEditor, materialProperty);

    }
    void Show()
    {
        EditorGUI.BeginChangeCheck();
        materialEditor.LightmapEmissionProperty();
        EditorGUI.EndChangeCheck();
    }
}
