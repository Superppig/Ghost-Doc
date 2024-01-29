using UnityEngine;

public interface IPlayer
{
    void TakeDamage(float damage);
    void UseEnerge(float energe);
    float GetHealth();
    float GetEnerge();
    
    //消耗失败音效
    void TakeEnergeFailAudio();
    
    //获取速度
    Quaternion GetOriRotation();
    Vector3 GetSpeed();
}