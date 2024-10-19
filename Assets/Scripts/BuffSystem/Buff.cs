using UnityEngine;

public class Buff
{
    public float buffTime;
    public float currentTime;
    //攻击力加成
    public float attackAddition;
    //攻击力乘积
    public float attackMultiplication;
    //速度加成
    public float speedAddition;
    //速度乘积
    public float speedMultiplication;
    //回复血量
    public float healthRecovery;
    
    public Buff(float buffTime, float attackAddition, float attackMultiplication, float healthRecovery,float speedAddition,float speedMultiplication)
    {
        this.buffTime = buffTime;
        this.attackAddition = attackAddition;
        this.attackMultiplication = attackMultiplication;
        this.healthRecovery = healthRecovery;
        this.speedAddition = speedAddition;
        this.speedMultiplication = speedMultiplication;
    }
    
    
    public bool IsOver()
    {
        return currentTime <= 0;
    }
    public bool DecreaseTime(float time)
    {
        currentTime -= time;
        currentTime = Mathf.Clamp(currentTime, 0f, buffTime);
        return IsOver();
    }
}