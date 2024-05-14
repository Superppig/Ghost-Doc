using System;
using System.Collections.Generic;
using UnityEngine;
using Services;
using Services.ObjectPools;

public class WeaponManager : MonoBehaviour
{
    private IObjectManager objectManager;
    private IMyObject currentWeapon;
    private int lastGunIndex;
    public List<string> gunNames;
    public List<string> meleeNames;
    private WeaponType currentType;

    private void Awake()
    {
        objectManager = ServiceLocator.Get<IObjectManager>();
    }
    public void WeaponInit(List<string> gunNames ,List<string> meleeNames,WeaponType type,int index)
    {
        this.gunNames= gunNames;
        this.meleeNames = meleeNames;
        currentType = type;

        if (currentType == WeaponType.Gun)
        {
            currentWeapon = objectManager.Activate(gunNames[index], transform.position, transform.rotation.eulerAngles, transform);
        }
        else
        {
            currentWeapon = objectManager.Activate(meleeNames[index], transform.position, transform.rotation.eulerAngles, transform);
        }
    }
    
    public void WeaoponSwitch(WeaponType type, int index)
    {
        currentType = type;
        currentWeapon.Recycle();
        if (currentType == WeaponType.Gun)
        {
            currentWeapon = objectManager.Activate(gunNames[index], transform.position, transform.rotation.eulerAngles, transform);
        }
        else
        {
            currentWeapon = objectManager.Activate(meleeNames[index], transform.position, transform.rotation.eulerAngles, transform);
        }
    }

}