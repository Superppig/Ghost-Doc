using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum PopUpTextType
{
    GunDamage,
}

public class PopUpTextManager
{
    private const string PopUpTextSettingPath = "Assets/Resources/UI/PopUpTextSetting.asset";
    private const string PopUpTextPerfabPath = "Assets/Resources/UI/PopText.prefab";
    private const string PopUpTextGroup = "PopUpText";

    private static PopUpTextSetting _popUpTextSetting;
    private static bool _isInitialized;

    public static void Initialize()
    {
        if (_isInitialized)
            return;
        _popUpTextSetting = Resources.Load<PopUpTextSetting>(PopUpTextSettingPath);
        if (_popUpTextSetting == null)
        {
            Debug.LogError("error: PopUpTextSetting is null");
            return;
        }

        _isInitialized = true;
    }

    /// <summary>
    /// 显示跳字
    /// </summary>
    public static void ShowDamageText(Transform actorTransform, int damage, PopUpTextType type,
        Vector3 hitVelocity = default)
    {
        if (actorTransform == null)
        {
            Debug.LogError("error: actorTransform is null");
            return;
        }

        var toRight = VelocityToRight(hitVelocity)?1:-1;
        
    }
    
    private static void CreateDamgePopUpText(int damage, PopUpTextType type, Vector3 position, int toRight)
    {
        var textAsset = type switch
        {
            PopUpTextType.GunDamage => _popUpTextSetting.gunDamage,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "error: PopUpTextType is not defined")
        };
        
        var entity = GameObject.Instantiate(Resources.Load<GameObject>(PopUpTextPerfabPath), position, Quaternion.identity);
    }


    private static bool VelocityToRight(Vector3 velocity)
    {
        if (velocity == default)
            return Random.Range(0, 1f) > 0.5f;
        return velocity.x > 0;
    }
}