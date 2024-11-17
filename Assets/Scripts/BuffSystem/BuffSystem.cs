using System;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType
{
    KenKen,
    Zombie,
    Remote,
}
public class BuffSystem : MonoBehaviour
{
    private static BuffSystem m_Instance;
    public static BuffSystem Instance
    {
        get
        {
            return m_Instance;
        }
    }
    private Dictionary<BuffType, Buff> m_BuffDict = new Dictionary<BuffType, Buff>();
    private List<BuffType> m_BuffList = new List<BuffType>(){
        BuffType.KenKen,
        BuffType.Zombie,
        BuffType.Remote,
    };
    private Player m_Player;
    private PlayerAttack m_PlayerAttack;

    private BuffType currentGun;
    private float gunTimer;
    private bool isOrigin = true;
    
    private float  totalSpeedAddition = 0f;
    private float totalSpeedMultiplication = 1f;
    private float totalAttackAddition = 0f;
    private float totalAttackMultiplication = 1f;
    private void Awake()
    {
        m_Instance = this;
        
        m_BuffDict.Add(BuffType.KenKen, new Buff(1,0,1,0,0,1));
        m_BuffDict.Add(BuffType.Zombie, new Buff(5,0,2,100,0,2));
        m_BuffDict.Add(BuffType.Remote, new Buff(10,0,1,100,0,1.5f));
    }

    private void Start()
    {
        m_Player = FindObjectOfType<Player>();
        m_PlayerAttack = m_Player.GetComponent<PlayerAttack>();
    }

    private void Update()
    {
        BuffUpdate();
        
        gunTimer -= Time.deltaTime;
        gunTimer = Mathf.Clamp(gunTimer, 0, 100);
        if (gunTimer<=0f &&!isOrigin)
        {
            m_PlayerAttack.SwitchWeapon(WeaponType.Gun, 0);
            isOrigin = true;
        }
    }

    private void BuffUpdate()
    {
        totalSpeedAddition = 0f;
        totalSpeedMultiplication = 1f;
        totalAttackAddition = 0f;
        totalAttackMultiplication = 1f;
        foreach (var index in m_BuffList)
        {
            if (!m_BuffDict[index].IsOver())
            {
                if (!m_BuffDict[index].DecreaseTime(Time.deltaTime))
                {
                    totalAttackAddition += m_BuffDict[index].attackAddition;
                    totalAttackMultiplication *= m_BuffDict[index].attackMultiplication;
                    totalSpeedAddition += m_BuffDict[index].speedAddition;
                    totalSpeedMultiplication *= m_BuffDict[index].speedMultiplication;
                }
            }
        }
    }
    
    public float GetBuffedSpeed(float speed)
    {
        return totalSpeedMultiplication * (totalSpeedAddition+speed) ;
    }
    public float GetBuffedAttack(float attack)
    {
        return totalAttackMultiplication * (totalAttackAddition+attack);
    }
    
    
        
    public void ActivateBuff(BuffType buffType)
    {
        m_BuffDict[buffType].currentTime = m_BuffDict[buffType].buffTime;
        m_Player.RecoverHealth(m_BuffDict[buffType].healthRecovery);
        UpdateGun(buffType);
    }

    private void UpdateGun(BuffType buffType)
    {
        gunTimer = m_BuffDict[buffType].buffTime;
        switch (buffType)
        {
            case BuffType.KenKen:
                break;
            case BuffType.Zombie:
                break;
            case BuffType.Remote:
                m_PlayerAttack.gunIndex = 1;
                m_PlayerAttack.SwitchWeapon(WeaponType.Gun, 1);
                isOrigin = false;
                break;
        }
    }
}
