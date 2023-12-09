using System;
using System.Collections;
using System.Threading;
using Cinemachine;
using UnityEngine;
using DG.Tweening;

public class PlayerAttack : MonoBehaviour
{
    public Gun[] Guns;
    [Header("当前武器类别")]
    public int index = 0;
    
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
        SwitchWeapon(WeaponType.Gun,index);
    }

    private void SwitchWeapon(WeaponType type,int index)
    {
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
                
                break;
        }
    }
}
