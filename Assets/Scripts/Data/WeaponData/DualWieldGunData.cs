using UnityEngine;


[CreateAssetMenu(fileName = "DualWieldGunData", menuName = "ScriptableObject/双持冲锋枪", order = 2)]
public class DualWieldGunData : GunData
{
    [Header("射击属性")] public LayerMask enemyMask;
    public float damageRate;
    public float maxShootDistance;
    public float fireRate = 700f; //射速

    public float bulletCircleRadius; //圆面的半径
    public float bulletCircleDistance; //圆面的距离
    [Header("特殊属性")] public float temperatureMax = 100f; //温度上限
    public float temperatureUpSpeed = 50f; //温度上升速度
    public float temperatureDownSpeed = 30f; //温度下降速度
    public float temperatureFrozenTime = 0.3f; //冷却时间

    [Header("子弹(激光)")] public LineRenderer bullet;
    public float lineWindth = 0.1f;
    public float bulletTime;
    [Header("相机")] public float impulseTime;
    public float impulseAmplitude;

    [Header("效果")] public ParticleSystem hitEenemyParticle;
    public ParticleSystem hitBuildingParticle;
    public ParticleSystem fireParticle;
}