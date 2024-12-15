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
        else
        {
            currentGun = weaponManager.GetComponentInChildren<Gun>();
        }
    }
    

    private void Update()
    {
       PlayerInput();
    }
    
    private void PlayerInput()
    {

    }

    public void SwitchWeapon(WeaponType type, int index)
    {
        currentType = type;
        weaponManager.WeaoponSwitch(type,index);
        switch (type)
        {
            case WeaponType.Gun:
                currentGun = weaponManager.GetComponentInChildren<Gun>();
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
                player.gunModel = currentMelee.transform;
                player.gunTrans = currentMelee.transform;
                break;
        }
        switchTimer = 0;
    }

    public void GunJumpAnim()
    {
        currentGun.gunAnimatorController.ImpulsePlay("jump3");
    }
}
