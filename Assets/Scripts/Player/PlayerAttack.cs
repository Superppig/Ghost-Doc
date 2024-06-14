using System.Collections.Generic;
using UnityEngine;


public enum WeaponType
{
    Gun,//枪械
    Melee//近战
}
public class PlayerAttack : MonoBehaviour
{
    public List<string> gunNames;
    public List<string> meleeNames;
    [Header("当前武器类别")]
    public int gunIndex = 0;
    public int meleeIndex = 0;
    [Header("切换设置")]
    public float switchTime = 0.5f;
    private float switchTimer;
    
    [SerializeField]
    private WeaponType currentType;
    private Player player;
    private Transform weaponParent;
    
    [SerializeField]
    private Melee currentMelee;
    [SerializeField]
    private Gun currentGun;
    
    public WeaponManager weaponManager;


    public void Start()
    {
        player = GetComponent<Player>();
        weaponParent = Camera.main.transform.Find("ArmPivotParent").Find("ArmShakeParent");
        weaponManager  = Instantiate(weaponManager,weaponParent);
        weaponManager.transform.localPosition = Vector3.zero;
        weaponManager.transform.localRotation = Quaternion.Euler(0,0,0);
        weaponManager.WeaponInit(gunNames,meleeNames,currentType,gunIndex);
        if(weaponManager==null)
            Debug.LogWarning("weaponManager is null");
    }
    

    private void Update()
    {
        PlayerInput();
    }
    
    private void PlayerInput()
    {
        switchTimer+= Time.deltaTime;
        switchTimer = Mathf.Clamp(switchTimer, 0, switchTime);
        //左键打断收刀
        if (Input.GetMouseButtonDown(0))
        {
            if (currentType == WeaponType.Melee)
            {
                if (currentMelee.state==Melee.WeaponState.Retracking)
                {
                    SwitchWeapon(WeaponType.Gun,gunIndex);
                    player.blackboard.isHoldingMelee = false;     
                    //同时让gun第一次射击
                    currentGun.firstFire = true;
                }
            }
        }
        
        //右键为近战
        if (Input.GetMouseButtonDown(1)&& meleeNames.Count>0)
        {
            if(currentType==WeaponType.Gun)
                SwitchWeapon(WeaponType.Melee,meleeIndex);
        }
        //近战攻击后切回枪械
        if (currentType == WeaponType.Melee && currentMelee.hasAttack)
        {
            SwitchWeapon(WeaponType.Gun,gunIndex);
        }
        
        //鼠标滚轮相关
        if (switchTimer >= switchTime)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                if (currentType == WeaponType.Gun)
                {
                    if (gunNames.Count>1)
                    {
                        gunIndex++;
                        if (gunIndex >= gunNames.Count)
                        {
                            gunIndex = 0;
                        }
                        SwitchWeapon(WeaponType.Gun,gunIndex);
                    }
                }
                else
                {
                    if (meleeNames.Count>1)
                    {
                        meleeIndex++;
                        if (meleeIndex >= meleeNames.Count)
                        {
                            meleeIndex = 0;
                        }
                        SwitchWeapon(WeaponType.Melee,meleeIndex);
                    }
                }
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                if (currentType == WeaponType.Gun)
                {
                    if (gunNames.Count>1)
                    {
                        gunIndex--;
                        if (gunIndex < 0)
                        {
                            gunIndex =gunNames.Count-1;
                        }
                        SwitchWeapon(WeaponType.Gun,gunIndex);
                    }
                }
                else
                {
                    if (meleeNames.Count>1)
                    {
                        meleeIndex--;
                        if (meleeIndex < 0)
                        {
                            meleeIndex = meleeNames.Count-1;
                        }
                        SwitchWeapon(WeaponType.Melee,meleeIndex);
                    }
                }
            }
        }
    }

    private void SwitchWeapon(WeaponType type, int index)
    {
        currentType = type;
        weaponManager.WeaoponSwitch(type,index);
        switch (type)
        {
            case WeaponType.Gun:
                currentGun = weaponParent.GetComponentInChildren<Gun>();
                player.gunModel = currentGun.model;
                player.gunTrans = currentGun.trans;
                //安全措施
                if (player.blackboard.isBlocking)
                {
                    player.blackboard.isBlocking = false;
                }
                if (player.blackboard.isMeleeAttacking)
                {
                    player.blackboard.isMeleeAttacking = false;
                }
                break;
            case WeaponType.Melee:
                currentMelee = weaponParent.GetComponentInChildren<Melee>();
                player.gunModel = currentMelee.transform;
                player.gunTrans = currentMelee.transform;
                break;
        }
        switchTimer = 0;
    }
}
