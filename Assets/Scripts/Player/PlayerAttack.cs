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
