using UnityEngine;

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
    
    private Melee currentMelee;
    private Gun currentGun;
 
    
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
        weaponParent = Camera.main.transform;
        //初始化
        SwitchWeapon(WeaponType.Gun,gunIndex);
    }

    private void Update()
    {
        PlayerInput();
    }
    
    private void PlayerInput()
    {
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
        if (Input.GetMouseButtonDown(1))
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
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (currentType == WeaponType.Gun)
            {
                if (Guns.Length>1)
                {
                    gunIndex++;
                    if (gunIndex >= Guns.Length)
                    {
                        gunIndex = 0;
                    }
                    SwitchWeapon(WeaponType.Gun,gunIndex);
                }
            }
            else
            {
                if (Melees.Length>1)
                {
                    meleeIndex++;
                    if (meleeIndex >= Melees.Length)
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
                if (Guns.Length>1)
                {
                    gunIndex--;
                    if (gunIndex < 0)
                    {
                        gunIndex = Guns.Length-1;
                    }
                    SwitchWeapon(WeaponType.Gun,gunIndex);
                }
            }
            else
            {
                if (Melees.Length>1)
                {
                    meleeIndex--;
                    if (meleeIndex < 0)
                    {
                        meleeIndex = Melees.Length-1;
                    }
                    SwitchWeapon(WeaponType.Melee,meleeIndex);
                }
            }
        }
    }

    private void SwitchWeapon(WeaponType type,int index)
    {
        currentType= type;
        Destroy(currentWeapon);
        switch (type)
        {
            case WeaponType.Gun:
                currentGun = Instantiate(Guns[index],weaponParent,false);
                currentWeapon=currentGun.gameObject;
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
                currentMelee = Instantiate(Melees[index],weaponParent,false);
                currentWeapon=currentMelee.gameObject;
                player.gunModel = currentMelee.transform;
                player.gunTrans = currentMelee.transform;
                break;
        }
    }
}
