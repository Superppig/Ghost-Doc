using UnityEngine;

namespace Data.WeaponData
{
    [CreateAssetMenu(fileName = "ShotGunData", menuName = "ScriptableObject/霰弹枪", order = 1)]

    public class ShotGunData : GunData
    {
        [Header("射击属性")]
        public LayerMask enemyMask;
        public float damageRate;
        public float maxShootDistance;//最大射程
        public int bulletCount;//子弹数量
        public float bulletMinDistance;//子弹最小距离
        public float bulletCircleRadius;//圆面的半径
        public float bulletCircleDistance;//圆面的距离
        
        [Header("子弹(激光)")]
        public LineRenderer bullet;
        public float lineWindth=0.1f;

        public float bulletTime;
        public float fireRate = 700f;//射速
        
        [Header("相机")] 
        public float impulseTime;
        public float impulseAmplitude;
        
        [Header("效果")] 
        public ParticleSystem hitEenemyParticle;
        public ParticleSystem hitBuildingParticle;
        public ParticleSystem fireParticle;
    }
}