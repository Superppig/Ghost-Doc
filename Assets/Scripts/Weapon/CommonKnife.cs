using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;


public class CommonKnife : Melee
{
    protected override void Start()
    {
        AttackAnimTime=0.25f;
        base.Start();
    }
    
    protected override void Update()
    {
        base.Update();
    }
    //Attack时的相机效果
    public void AttackCamChange()
    {
        camTrans.DOLocalRotate(new Vector3(30, 0, 0), 0.2f);
        StartCoroutine(FinishCamChange());
    }
    IEnumerator FinishCamChange()
    {
        yield return new WaitForSeconds(0.1f);
        camTrans.DOLocalRotate(Vector3.zero, 0.1f);
    }
}