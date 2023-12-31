using System;
using System.Collections;
using System.Threading;
using Cinemachine;
using UnityEngine;
using DG.Tweening;

public class PlayerAttack : MonoBehaviour
{
    public Gun[] Guns;
    public Melee[] Melees;
    [Header("当前武器类别")]
    public int gunIndex = 0;
    public int meleeIndex = 0;
    
    private WeaponType currentType;
    private Player player;
    private Transform weaponParent;
    private enum WeaponType
    {
        Gun,//枪械
        Melee//近战
    }
    
    //逻辑变量
    private GameObject currentWeapon;

    private void Start()
    {
        player = GetComponent<Player>();
        weaponParent = player.playerBlackboard.cam.GetComponent<Transform>();
        //初始化
        SwitchWeapon(WeaponType.Gun,gunIndex);
    }

    private void Update()
    {
        //初步实现切换武器
        if (Input.GetKeyDown(KeyCode.V))
        {
            if(currentType==WeaponType.Gun)
                SwitchWeapon(WeaponType.Melee,meleeIndex);
            else
                SwitchWeapon(WeaponType.Gun,gunIndex);
        }
    }

    private void SwitchWeapon(WeaponType type,int index)
    {
        currentType= type;
        Destroy(currentWeapon);
        switch (type)
        {
            case WeaponType.Gun:
                Gun gun = Instantiate(Guns[index],weaponParent,false);
                currentWeapon=gun.gameObject;
                player.playerBlackboard.gunModel = gun.model;
                player.playerBlackboard.gunTrans = gun.trans;
                break;
            case WeaponType.Melee:
                Melee melee = Instantiate(Melees[index],weaponParent,false);
                currentWeapon=melee.gameObject;
                player.playerBlackboard.gunModel = melee.transform;
                player.playerBlackboard.gunTrans = melee.transform;
                break;
        }
    }
}
