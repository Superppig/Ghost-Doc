using UnityEngine;
using Cinemachine;


[CreateAssetMenu(fileName = "CommonGunData", menuName = "ScriptableObject/普通左轮", order = 0)]
public class CommonGundata : GunData
{
    [Header("射击属性")] 
    public LayerMask enemyMask;
    public LayerMask pyhsicsMask;
    public float damageRate;
    public float maxShootDistance;
    [Header("子弹(激光)")] public LineRenderer bullet;

    public float lineWindth = 0.1f;

    public float bulletTime;

    public float fireRate = 700f; //射速

    [Header("后坐力")] public float verOffMin = 2f; //水平偏移量
    public float verOffMax = 5f; //水平偏移量
    public float horOff = 1f; //垂直偏移量

    [Header("相机")] public float impulseTime;
    public float impulseAmplitude;

    [Header("效果")] public ParticleSystem hitEenemyParticle;
    public ParticleSystem hitBuildingParticle;
    public ParticleSystem fireParticle;
    public float hitScale = 0.5f;
    public float fireScale = 0.5f;
}